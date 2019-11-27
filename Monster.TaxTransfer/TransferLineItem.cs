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
        public decimal TaxableAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal Total => LineAmount + TaxAmount;

        public List<TransferTaxLine> TaxLines { get; set; }



        public TransferLineItem()
        {
            TaxLines = new List<TransferTaxLine>();
        }
    }
}
