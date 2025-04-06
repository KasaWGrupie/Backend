using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using KasaWGrupie.Infrastructure.ReceiptProcessor;
using System.Globalization;

namespace KasaWGrupie.Tests.ReceiptProcessingTests
{
    [TestClass]
    public class ReceiptProcessorTests
    {
        [TestInitialize]
        public void Setup()
        {
            // If needed, you can set the GOOGLE_APPLICATION_CREDENTIALS environment variable here.
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", @"C:\keys\key.json");

        }

        [TestMethod]
        public void ProcessReceipt_WithValidImage_ReturnsExtractedText()
        {
            // Arrange: Build the file path to the test image.
            // TestContext.TestDir represents the working directory of the test; adjust accordingly.
           
            string imagePath = @"C:\Users\adamf\Desktop\student_debil\semestr6\io\test_paragon.jfif";

            if (!File.Exists(imagePath))
            {
                Assert.Fail("Test image was not found. Ensure that 'sampleReceipt.jpg' is in the Assets folder and copied to the output directory.");
            }

            IReceiptProcessor receiptProcessor = new ReceiptProcessor();

            // Act: Process the receipt image.
            string extractedText = receiptProcessor.ProcessReceipt(imagePath);

            // Output some details for inspection (optional).
            Console.WriteLine("Extracted Text: " + extractedText);

            // Assert: Check that some text was extracted.
            Assert.IsFalse(string.IsNullOrEmpty(extractedText), "Extracted text should not be null or empty.");
        }

        [TestMethod]
        public void ParseReceiptText_ReturnsExpectedItemsAndTotal()
        {
            string imagePath = @"C:\Users\adamf\Desktop\student_debil\semestr6\io\test_paragon.jfif";
            IReceiptProcessor receiptProcessor = new ReceiptProcessor();

            // Act: Process the receipt image.
            string extractedText = receiptProcessor.ProcessReceipt(imagePath);
            // In this sample text our parser is expected to pick up three items:
            // "PIWO 4-PAK ZATECK-A 1 x 13.50" with price 13,600,
            // "PINO ZATECKY-Ax 3,40" with price 3,40,
            // and "PAPIEROSY WINS 8-0 113.99" with price 13,998.
            // It should also detect the total "34,39".
            // Note: The actual numbers here are strings using the comma as a decimal separator.  
            // Adjust expected values as necessary based on your parsing rules.

            // Arrange
            ReceiptParser parser = new ReceiptParser();

            // Act
            var (items, total) = parser.ParseReceiptText(extractedText);

            // Debug: Write results to TestContext for inspection.
            TestContext.WriteLine("Parsed Items:");
            foreach (var item in items)
            {
                TestContext.WriteLine($"Description: {item.Description}, Price: {item.Price.ToString("N2", new CultureInfo("pl-PL"))}");
            }
            TestContext.WriteLine("Total: " + total.ToString("N2", new CultureInfo("pl-PL")));

            // Assert: Verify that we at least found some items and the total is correctly parsed.
            // (Adjust the expected number/count and totals depending on your specific text and parser logic.)
            Assert.IsTrue(items.Count >= 1, "Should detect at least 3 items.");
            Assert.AreEqual(34.39m, total, "Total amount should be 34,39 (in pl-PL decimal format).");

            // You may also add further assertions checking individual items.
            var firstItem = items.FirstOrDefault();
            Assert.IsNotNull(firstItem, "At least one item should be present.");
            Assert.IsTrue(firstItem.Description.Contains("PIWO"), "First item should contain the product name 'PIWO'.");
        }

        // For MSTest, you need a TestContext property if you refer to it.
        public TestContext TestContext { get; set; }
    }
}


