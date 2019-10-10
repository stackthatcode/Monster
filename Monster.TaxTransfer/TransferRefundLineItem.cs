using System;

namespace Monster.TaxTransfer
{
    [Obsolete("Unnecessary detail - it's the consumer's job to aggregate!")]
    public class TransferRefundLineItem
    {
        public string ExternalRefNbr { get; set; }
        public string InventoryID { get; set; }
        public bool IsTaxable { get; set; }
        public decimal SubTotal { get; set; }
        public decimal TotalTax { get; set; }
    }
}
