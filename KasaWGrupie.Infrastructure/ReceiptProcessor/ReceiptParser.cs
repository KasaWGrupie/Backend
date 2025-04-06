using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;


namespace KasaWGrupie.Infrastructure.ReceiptProcessor
{

    public class ReceiptItem
    {
        public string Description { get; set; }
        public decimal Price { get; set; }
    }
    public class ReceiptParser
    {
        // Parse the raw OCR text from the receipt.
        public (List<ReceiptItem> Items, decimal Total) ParseReceiptText(string extractedText)
        {
            var items = new List<ReceiptItem>();
            decimal total = 0m;
            var culture = new CultureInfo("pl-PL"); // Adjust if your receipt uses a different number format.

            // Split the text into lines and remove empty lines.
            var lines = extractedText
                .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .ToList();

            // Loop over lines looking for price-like numbers and keywords.
            for (int i = 0; i < lines.Count; i++)
            {
                // Detect the total cost if the current line contains a keyword
                if (lines[i].IndexOf("suma", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    if (i + 1 < lines.Count)
                    {
                        if (TryParsePrice(lines[i + 1], culture, out decimal parsedTotal))
                        {
                            total = parsedTotal;
                            i++; // Skip next line since it was used as total.
                            continue;
                        }
                    }
                }

                // Attempt to parse the current line as a price.
                if (TryParsePrice(lines[i], culture, out decimal price))
                {
                    // If a price number is found, assume that the previous line was a description.
                    if (i - 1 >= 0)
                    {
                        string description = lines[i - 1];
                        items.Add(new ReceiptItem { Description = description, Price = price });
                    }
                }
            }

            return (items, total);
        }

        // Helper method to parse a money value from a string.
        private bool TryParsePrice(string input, CultureInfo culture, out decimal price)
        {
            // Remove any extraneous symbols, e.g., spaces or currency signs.
            string clean = input.Replace(" ", "")
                                .Replace("H", "")  // Remove accidental trailing letters, if needed.
                                .Replace("x", ""); // You can add more cleaning rules as necessary.
            return decimal.TryParse(clean, NumberStyles.Number, culture, out price);
        }
    }
}

