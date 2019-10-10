using System.Collections.Generic;
using System.Linq;

namespace Monster.TaxTransfer
{
    public class Transfer
    {
        public string ExternalRefNbr { get; set; }
        public List<TransferLineItem> LineItems { get; set; }
        public TransferFreight Freight { get; set; }
        public List<TransferRefund> Refunds { get; set; }

        public Transfer()
        {
            LineItems = new List<TransferLineItem>();
            Freight = new TransferFreight();
            Refunds = new List<TransferRefund>();
        }

        public decimal TotalTax 
                    => LineItems.Sum(x => x.TaxAmount) + Freight.TaxAmount;
        public decimal TotalLineItemTaxAfterRefunds 
                    => LineItems.Sum(x => x.TaxAmount) - Refunds.Sum(x => x.TotalLineItemsTax);
        public decimal TotalFreightTaxAfterRefunds 
                    => Freight.TaxAmount - Refunds.Sum(x => x.FreightTax);
        public decimal TotalTaxableLineAmountsAfterRefund
                    => LineItems.Sum(x => x.TaxableAmount) - Refunds.Sum(x => x.TotalTaxableLineAmounts);
        public decimal TotalTaxableFreightAfterRefund
                    => Freight.TaxableAmount - Refunds.Sum(x => x.TaxableFreightAmount);

        public TransferLineItem LineItem(string inventoryID)
        {
            return LineItems.FirstOrDefault(x => x.InventoryID == inventoryID);
        }
    }
}
