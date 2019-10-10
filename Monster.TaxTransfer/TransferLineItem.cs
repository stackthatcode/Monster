using System.Collections.Generic;

namespace Monster.TaxTransfer
{
    public class TransferLineItem
    {
        public string ExternalRefNbr { get; set; }
        public string InventoryID { get; set; }
        public int OriginalQuantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal OriginalLineAmount => OriginalQuantity * UnitPrice;

        public bool IsTaxable { get; set; }
        public decimal OriginalTotalTax { get; set; }
        public List<TransferTaxLine> TaxLines { get; set; }


        public TransferLineItem()
        {
            TaxLines = new List<TransferTaxLine>();
        }
    }
}
