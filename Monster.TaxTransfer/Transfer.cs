using System;
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

        public decimal TotalLineItemTax => LineItems.Sum(x => x.TotalTax);
        public decimal TotalTax => TotalLineItemTax + Freight.TotalTax;

        public decimal TotalLineItemTaxAfterRefunds => TotalTax - Refunds.Sum(x => x.NonFreightTax);
        public decimal TotalFreightTaxAfterRefunds => Freight.TotalTax - Refunds.Sum(x => x.NonFreightTax);
        public decimal TotalTaxAfterRefunds => TotalLineItemTaxAfterRefunds + TotalFreightTaxAfterRefunds;


        public decimal CalcSplitShipmentTaxes(string inventoryId, int quantity)
        {
            var lineitem = LineItems.FirstOrDefault(x => x.InventoryID == inventoryId);
            if (lineitem == null)
            {
                throw new Exception($"Unable to locate Inventory ID == {inventoryId}");
            }
            else
            {
                return lineitem.CalcSplitShipmentTaxes(quantity);
            }
        }
    }
}
