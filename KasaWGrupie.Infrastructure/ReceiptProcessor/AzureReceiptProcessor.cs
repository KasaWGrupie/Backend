using System;
using System.IO;
using System.Threading.Tasks;
using Azure;
using Azure.AI.FormRecognizer.DocumentAnalysis;
using Microsoft.Extensions.Options;
using KasaWGrupie.Core;
using KasaWGrupie.Infrastructure;


namespace KasaWGrupie.Infrastructure.ReceiptProcessor
{



    public class AzureReceiptProcessor : IReceiptProcessor
    {
        private readonly DocumentAnalysisClient _client;

        public AzureReceiptProcessor(IOptions<DocumentIntelligenceOptions> options)
        {

            var endpoint = new Uri(options.Value.Endpoint);
            var credential = new AzureKeyCredential(options.Value.ApiKey);
            _client = new DocumentAnalysisClient(endpoint, credential);
        }

        public async Task<ReceiptParseResult> AnalyzeAsync(Stream receiptImage)
        {
            // 1) Kick off the prebuilt receipt model and wait until it's done
            AnalyzeDocumentOperation operation = await _client.AnalyzeDocumentAsync(
                WaitUntil.Completed,      // Poll until done
                "prebuilt-receipt",       // Model ID
                receiptImage              // Your image stream
            );

            // 2) Grab the single receipt document (there's only one in this model)
            var analysis = operation.Value;
            var receipt = analysis.Documents.FirstOrDefault()
                            ?? throw new InvalidOperationException("No receipt was recognized.");

            var fields = receipt.Fields;
            var parsed = new ReceiptParseResult();

            // 3) MerchantName (string)
            if (fields.TryGetValue("MerchantName", out var mField)
                && mField.FieldType == DocumentFieldType.String)
            {
                parsed.MerchantName = mField.Value.AsString();
            }

            // 4) TransactionDate (date)
            if (fields.TryGetValue("TransactionDate", out var dField)
                && dField.FieldType == DocumentFieldType.Date)
            {
                parsed.TransactionDate = dField.Value.AsDate();
            }

            // 5) Total (double)
            if (fields.TryGetValue("Total", out var tField)
                && tField.FieldType == DocumentFieldType.Double)
            {
                parsed.Total = tField.Value.AsDouble();
            }

            // 6) Items (list of dictionary)
            if (fields.TryGetValue("Items", out var iField)
                && iField.FieldType == DocumentFieldType.List)
            {
                foreach (var itemField in iField.Value.AsList())
                {
                    if (itemField.FieldType != DocumentFieldType.Dictionary)
                        continue;

                    var dict = itemField.Value.AsDictionary();
                    var line = new ReceiptItem();

                    if (dict.TryGetValue("Description", out var desc)
                        && desc.FieldType == DocumentFieldType.String)
                    {
                        line.Description = desc.Value.AsString();
                    }

                    if (dict.TryGetValue("Quantity", out var qty)
                        && qty.FieldType == DocumentFieldType.Double)
                    {
                        line.Quantity = qty.Value.AsDouble();
                    }

                    if (dict.TryGetValue("TotalPrice", out var price)
                        && price.FieldType == DocumentFieldType.Double)
                    {
                        line.Price = price.Value.AsDouble();
                    }

                    parsed.Items.Add(line);
                }
            }

            return parsed;
        }
    }
}
