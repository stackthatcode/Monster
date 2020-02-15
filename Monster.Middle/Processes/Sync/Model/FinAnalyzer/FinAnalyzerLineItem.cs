using System.Collections.Generic;

namespace Monster.Middle.Processes.Sync.Model.FinAnalyzer
{
    public class FinAnalyzerLineItem
    {
        public string ExternalRefNbr { get; set; }
        public string InventoryID { get; set; }
        public int Quantity { get; set; }   
        public decimal UnitPrice { get; set; }
        public decimal LineAmount => Quantity * UnitPrice;
        public decimal TaxableAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal Total => LineAmount + TaxAmount;
        public bool IsTaxable => TaxAmount > 0m;
        public List<FinAnalyzerTaxLine> TaxLines { get; set; }


        public FinAnalyzerLineItem()
        {
            TaxLines = new List<FinAnalyzerTaxLine>();
        }

        public decimal OnTheFlyTaxableAmount(int quantity)
        {
            return IsTaxable ? quantity * UnitPrice : 0m;
        }
    }
}
