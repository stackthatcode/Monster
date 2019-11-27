using System.Collections.Generic;

namespace Monster.TaxTransfer
{
    public class TransferFreight
    {
        public string Description { get; set; }
        public decimal Price { get; set; }

        public decimal TaxAmount { get; set; }
        public decimal TaxableAmount { get; set; }
        public List<TransferTaxLine> TaxLines { get; set; }

        public TransferFreight()
        {
            TaxLines = new List<TransferTaxLine>();
        }
    }
}
