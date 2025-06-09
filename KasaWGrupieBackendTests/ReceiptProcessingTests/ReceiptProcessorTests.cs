using System;
using System.IO;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using KasaWGrupie.Infrastructure;
using KasaWGrupie.Infrastructure.ReceiptProcessor;
using Azure;
using System.Text.Json;

namespace KasaWGrupie.Tests
{
    [TestClass]
    public class ReceiptProcessorTests
    {
        private AzureReceiptProcessor _processor;
        public TestContext TestContext { get; set; }

        [TestInitialize]
        public void Setup()
        {
            DotNetEnv.Env.Load();
            var endpoint = Environment.GetEnvironmentVariable("AZURE_DOC_INTEL_ENDPOINT");
            var apiKey = Environment.GetEnvironmentVariable("AZURE_DOC_INTEL_KEY");

            if (string.IsNullOrEmpty(endpoint) || string.IsNullOrEmpty(apiKey))
                Assert.Inconclusive("Please set AZURE_DOC_INTEL_ENDPOINT and AZURE_DOC_INTEL_KEY.");

            var opts = Options.Create(new DocumentIntelligenceOptions
            {
                Endpoint = endpoint,
                ApiKey = apiKey
            });

            _processor = new AzureReceiptProcessor(opts);
        }

        [TestMethod]
        public void AnalyzeAsync_WithValidReceipt_ShouldReturnItemsAndTotal()
        {
            // Arrange
            string path = Path.Combine(AppContext.BaseDirectory, "TestData", "receipt-sample.jpg");
            Assert.IsTrue(File.Exists(path), $"Missing test data: {path}");
            using var stream = File.OpenRead(path);

            // Act
            var task = _processor.AnalyzeAsync(stream);
            task.Wait();
            ReceiptParseResult result = task.Result;

            // Log the full parsed object as JSON
            string json = JsonSerializer.Serialize(result, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            TestContext.WriteLine("=== Parsed Receipt Output ===");
            TestContext.WriteLine(json);

            // Also log key fields individually
            TestContext.WriteLine($"MerchantName: {result.MerchantName}");
            TestContext.WriteLine($"TransactionDate: {result.TransactionDate}");
            TestContext.WriteLine($"Total: {result.Total}");
            foreach (var item in result.Items)
            {
                TestContext.WriteLine($"  Item: {item.Description}, qty={item.Quantity}, price={item.Price}");
            }


            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(string.IsNullOrWhiteSpace(result.MerchantName), "MerchantName expected");
            Assert.IsTrue(result.TransactionDate.HasValue, "TransactionDate expected");
            Assert.IsTrue(result.Total > 0, "Total should be positive");
            CollectionAssert.AllItemsAreNotNull(result.Items);
            Assert.IsTrue(result.Items.Count > 0, "Expected at least one line item");
            foreach (var item in result.Items)
            {
                Assert.IsFalse(string.IsNullOrWhiteSpace(item.Description), "Item description expected");
                Assert.IsTrue(item.Price > 0, "Item price should be positive");
            }
        }

        [TestMethod]
        public async Task AnalyzeAsync_WithInvalidStream_ShouldThrowRequestFailedException()
        {
            // Arrange: random bytes that aren’t a receipt
            using var badStream = new MemoryStream(new byte[] { 0, 1, 2 });

            // Act & Assert
            await Assert.ThrowsExceptionAsync<RequestFailedException>(
                () => _processor.AnalyzeAsync(badStream),
                "Invalid content should trigger a RequestFailedException");
        }
    }
}
