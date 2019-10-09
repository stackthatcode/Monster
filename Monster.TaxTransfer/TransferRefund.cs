using System.Collections.Generic;
using System.Linq;

namespace Monster.TaxTransfer
{
    public class TransferRefund
    {
        public string ExternalSystemRefNbr { get; set; }
        public List<TransferLineItem> LineItems { get; set; }
        public TransferFreight Freight { get; set; }

        public decimal TotalTaxableAmount
            => LineItems.Sum(x => x.TaxableAmount) + Freight.TaxableAmount;
        public decimal TotalNonTaxableAmount
            => LineItems.Sum(x => x.NonTaxableAmount) + Freight.NonTaxableAmount;

        public decimal TotalTax => LineItems.Sum(x => x.TotalTax) + Freight.TotalTax;
    }
}

