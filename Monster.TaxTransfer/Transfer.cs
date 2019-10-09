using System.Collections.Generic;
using System.Linq;

namespace Monster.TaxTransfer
{
    public class Transfer
    {
        public string ExternalSystemRefNbr { get; set; }
        public List<TransferLineItem> LineItems { get; set; }
        public TransferFreight Freight { get; set; }
        public List<TransferRefund> Refunds { get; set; }

        public decimal TotalTaxableAmount 
            => LineItems.Sum(x => x.TaxableAmount) + Freight.TaxableAmount;
        public decimal TotalNonTaxableAmount 
            => LineItems.Sum(x => x.NonTaxableAmount) + Freight.NonTaxableAmount;
        public decimal TotalTax => LineItems.Sum(x => x.TotalTax) + Freight.TotalTax;

        public decimal TotalTaxableAmountAfterRefunds => TotalTaxableAmount - Refunds.Sum(x => x.TotalTaxableAmount);
        public decimal TotalNonTaxableAmountAfterRefunds => TotalNonTaxableAmount - Refunds.Sum(x => x.TotalNonTaxableAmount);
        public decimal TotalTaxAfterRefunds => TotalTax - Refunds.Sum(x => x.TotalTax);
    }
}
