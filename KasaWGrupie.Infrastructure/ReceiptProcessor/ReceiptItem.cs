using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasaWGrupie.Infrastructure.ReceiptProcessor
{
    public class ReceiptItem
    {
        public string Description { get; set; } = "";
        public double Quantity { get; set; }
        public double Price { get; set; }
    }
}
