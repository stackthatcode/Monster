using System.Collections.Generic;

namespace Monster.TaxTransfer
{
    public class TransferFreight
    {
        public string Description { get; set; }
        public decimal OriginalPrice { get; set; }

        public decimal OriginalTotalTax { get; set; }
        public bool IsTaxable { get; set; }
        public List<TransferTaxLine> TaxLines { get; set; }

        public TransferFreight()
        {
            TaxLines = new List<TransferTaxLine>();
        }
    }
}
