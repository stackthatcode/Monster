using System;
using System.Collections.Generic;
using System.Linq;

namespace Monster.TaxTransfer
{
    public class TransferFreight
    {
        public string Description { get; set; }
        public List<TransferTaxLine> TaxLines { get; set; }
        public bool IsTaxable { get; set; }

        public decimal TotalPrice { get; set; }
        public decimal TaxableAmount => IsTaxable ? TotalPrice : 0m;
        public decimal NonTaxableAmount => IsTaxable ? 0m : TotalPrice;

        public decimal TotalTax => TaxLines.Sum(x => Math.Round(x.Rate * TotalPrice, 2));
    }
}
