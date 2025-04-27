using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasaWGrupie.Infrastructure.ReceiptProcessor
{
    public class ReceiptParseResult
    {
        public string MerchantName { get; set; } = "";
        public DateTimeOffset? TransactionDate { get; set; }
        public double Total { get; set; }
        public List<ReceiptItem> Items { get; set; } = new();
    }
}
