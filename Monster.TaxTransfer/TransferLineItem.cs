using System.Collections.Generic;

namespace Monster.TaxTransfer
{
    public class TransferLineItem
    {
        public string ExternalRefNbr { get; set; }
        public string InventoryID { get; set; }
        public int Quantity { get; set; }   
        public decimal UnitPrice { get; set; }
        public decimal LineAmount => Quantity * UnitPrice;
        public bool IsTaxable { get; set; }
        public decimal TaxableAmount => IsTaxable ? LineAmount : 0m;
        public decimal TaxAmount { get; set; }

        public List<TransferTaxLine> TaxLines { get; set; }


        public TransferLineItem()
        {
            TaxLines = new List<TransferTaxLine>();
        }
    }
}
