using System.Collections.Generic;

namespace Monster.TaxTransfer.v2
{
    public class TaxSnapshotLineItem
    {
        public string ItemID { get; set; }
        public List<TaxSnapshotTaxLine> TaxLines { get; set; }
    }
}
