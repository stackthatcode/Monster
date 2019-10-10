using System.Collections.Generic;

namespace Monster.TaxTransfer
{
    public class TransferFreight
    {
        public string Description { get; set; }
        public decimal Price { get; set; }

        public decimal TaxAmount { get; set; }
        public bool IsTaxable { get; set; }
        public decimal TaxableAmount => IsTaxable ? Price : 0m;
        public List<TransferTaxLine> TaxLines { get; set; }

        public TransferFreight()
        {
            TaxLines = new List<TransferTaxLine>();
        }
    }
}
