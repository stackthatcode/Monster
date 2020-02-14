using System.Collections.Generic;

namespace Monster.TaxTransfer.v1
{
    public class TransferFreight
    {
        public string Description { get; set; }
        public decimal Price { get; set; }

        public decimal TaxAmount { get; set; }
        public decimal TaxableAmount { get; set; }
        public decimal Total => TaxAmount + Price;
        public List<TransferTaxLine> TaxLines { get; set; }

        public TransferFreight()
        {
            TaxLines = new List<TransferTaxLine>();
        }
    }
}
