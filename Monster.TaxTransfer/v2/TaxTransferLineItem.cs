using System.Collections.Generic;

namespace Monster.TaxTransfer.v2
{
    public class TaxTransferLineItem
    {
        public string ItemID { get; set; }
        public List<TaxTransferTaxLine> TaxLines { get; set; }
    }
}
