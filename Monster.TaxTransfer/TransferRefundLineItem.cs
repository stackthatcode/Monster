using System;

namespace Monster.TaxTransfer
{
    [Obsolete("Deferred - it's the consumer's job to aggregate - for now")]
    public class TransferRefundLineItem
    {
        public string ExternalRefNbr { get; set; }
        public string InventoryID { get; set; }
        public bool IsTaxable { get; set; }
        public decimal SubTotal { get; set; }
        public decimal TotalTax { get; set; }
    }
}
