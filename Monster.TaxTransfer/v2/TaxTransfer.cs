using System;
using System.Collections.Generic;
using System.Linq;

namespace Monster.TaxTransfer.v2
{
    public class TaxTransfer
    {
        public long ShopifyOrderId { get; set; }
        public List<long> ShopifyRefundIds { get; set; }

        public decimal NetTaxableFreight { get; set; }
        public decimal NetFreightTax { get; set; }
        public decimal NetTaxableAmount { get; set; }
        public decimal NetTotalTax { get; set; }

        public List<TaxTransferTaxLine> FreightTaxLines { get; set; }
        public List<TaxTransferLineItem> LineItems { get; set; }

        public TaxTransferCalc CalculateTax(
                string correctedItemCode, decimal taxableAmount, int quantity)
        {
            var lineItem = LineItems.First(x => x.ItemID == correctedItemCode);
            var output = new TaxTransferCalc();

            output.Name = $"{correctedItemCode} - qty {quantity}";
            output.TaxableAmount = taxableAmount;
            output.TaxAmount = lineItem.TaxLines.Sum(x => Math.Round(x.Rate * taxableAmount, 2));

            return output;
        }

        [Obsolete]
        public static decimal TruncateToHundreths(decimal value)
        {
            return Math.Truncate(100 * value) / 100;
        }

    }
}

