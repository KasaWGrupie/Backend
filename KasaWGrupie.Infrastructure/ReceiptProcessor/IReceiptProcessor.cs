using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasaWGrupie.Infrastructure.ReceiptProcessor
{
    public interface IReceiptProcessor
    {
        /// <summary>
        /// Analyze a receipt image and return parsed merchant, items and total.
        /// </summary>
        Task<ReceiptParseResult> AnalyzeAsync(Stream receiptImage);
    }
}
