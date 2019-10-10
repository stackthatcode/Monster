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

        public decimal OriginalTotalTax => LineItems.Sum(x => x.OriginalTotalTax) + Freight.OriginalTotalTax;
        public decimal TotalLineItemTaxAfterRefunds => LineItems.Sum(x => x.OriginalTotalTax) - Refunds.Sum(x => x.NonFreightTax);
        public decimal TotalFreightTaxAfterRefunds => Freight.OriginalTotalTax - Refunds.Sum(x => x.FreightTax);
        
        public TransferLineItem LineItem(string inventoryID)
        {
            return LineItems.FirstOrDefault(x => x.InventoryID == inventoryID);
        }
    }
}
