using System.Collections.Generic;
using System.Linq;

namespace Monster.TaxTransfer.v2
{
    public class TaxSnapshot
    {
        public long ShopifyOrderId { get; set; }
        public List<long> ShopifyRefundIds { get; set; }

        public decimal NetTaxableFreight { get; set; }
        public decimal NetFreightTax { get; set; }
        public decimal NetTaxableAmount { get; set; }
        public decimal NetTotalTax { get; set; }

        public List<TaxSnapshotTaxLine> FreightTaxLines { get; set; }
        public List<TaxSnapshotLineItem> LineItems { get; set; }

        public TaxSnapshotCalc CalculateTax(
                string correctedItemCode, decimal taxableAmount, int quantity)
        {
            var lineItem = LineItems.First(x => x.ItemID == correctedItemCode);
            var output = new TaxSnapshotCalc();

            output.Name = $"{correctedItemCode} - qty {quantity}";
            output.TaxableAmount = taxableAmount;
            output.TaxAmount = lineItem.TaxLines.Sum(x => x.Rate * taxableAmount);

            return output;
        }

    }
}

