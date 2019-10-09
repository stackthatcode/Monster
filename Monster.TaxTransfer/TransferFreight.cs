using System.Collections.Generic;

namespace Monster.TaxTransfer
{
    public class TransferFreight
    {
        public string Description { get; set; }
        public List<TransferTaxLine> TaxLines { get; set; }
        public bool IsTaxable { get; set; }

        public TransferFreight()
        {
            TaxLines = new List<TransferTaxLine>();
        }

        public decimal TotalPrice { get; set; }
        public decimal TaxableAmount => IsTaxable ? TotalPrice : 0m;
        public decimal NonTaxableAmount => IsTaxable ? 0m : TotalPrice;
        public decimal TotalTax => TaxLines.CalculateTaxes(TaxableAmount);
    }
}
