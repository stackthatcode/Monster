using System.Collections.Generic;

namespace Monster.TaxTransfer.v2
{
    public class TaxSnapshot
    {
        public long ShopifyOrderId { get; set; }
        public List<long> ShopifyRefundIds { get; set; }

        public List<TaxSnapshotLineItem> LineItems { get; set; }

        public decimal NetTaxableFreight { get; set; }
        public List<TaxSnapshotTaxLine> FreightTaxLines { get; set; }
        public decimal NetFreightTax { get; set; }

        public decimal NetTaxableAmount { get; set; }
        public decimal NetTotalTax { get; set; }
    }
}

