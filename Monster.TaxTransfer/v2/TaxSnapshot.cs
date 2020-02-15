using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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


    }
}

