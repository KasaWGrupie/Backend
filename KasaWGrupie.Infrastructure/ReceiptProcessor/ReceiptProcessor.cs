using Google.Cloud.Vision.V1;
using System;
using static System.Net.Mime.MediaTypeNames;




namespace KasaWGrupie.Infrastructure.ReceiptProcessor
{
    public interface IReceiptProcessor
    {
        string ProcessReceipt(string imagePath);
    }

    public class ReceiptProcessor : IReceiptProcessor
    {
        private readonly ImageAnnotatorClient _client;

        // Initialize the Vision client inside the constructor.
        public ReceiptProcessor()
        {
            // This will use the GOOGLE_APPLICATION_CREDENTIALS, or the default credentials of your project.
            _client = ImageAnnotatorClient.Create();
        }

        public string ProcessReceipt(string imagePath)
        {
            try
            {
                // Load the receipt image from a file.
                var image = Google.Cloud.Vision.V1.Image.FromFile(imagePath);

                // Use the DetectDocumentText method for a full-text OCR on documents.
                var response = _client.DetectDocumentText(image);

                // Optionally, add further processing here to extract specific fields from the OCR result.
                if (response != null)
                {
                    return response.Text;
                }
                else
                {
                    return "No text was detected in the receipt.";
                }
            }
            catch (Exception ex)
            {
                // In a real application, log the exception.
                return $"An error occurred while processing receipt: {ex.Message}";
            }
        }
    }
}

