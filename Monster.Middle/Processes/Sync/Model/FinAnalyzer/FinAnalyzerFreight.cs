using System.Collections.Generic;

namespace Monster.Middle.Processes.Sync.Model.FinAnalyzer
{
    public class FinAnalyzerFreight
    {
        public string Description { get; set; }
        public decimal Price { get; set; }

        public decimal TaxAmount { get; set; }
        public decimal TaxableAmount { get; set; }
        public decimal Total => TaxAmount + Price;
        public List<FinAnalyzerTaxLine> TaxLines { get; set; }

        public FinAnalyzerFreight()
        {
            TaxLines = new List<FinAnalyzerTaxLine>();
        }
    }
}
