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

        public decimal TotalTaxableLineAmountsAfterRefund
            => LineItems.Sum(x => x.TaxableAmount) - Refunds.Sum(x => x.TotalTaxableLineAmounts);
        public decimal TotalTaxableFreightAmountAfterRefund
            => Freight.TaxableAmount - Refunds.Sum(x => x.TaxableFreightAmount);

        public decimal TotalTax => LineItems.Sum(x => x.TaxAmount) + Freight.TaxAmount;
        public decimal TotalLineItemTaxAfterRefunds 
                    => LineItems.Sum(x => x.TaxAmount) - Refunds.Sum(x => x.TotalLineItemsTax);
        public decimal TotalFreightTaxAfterRefunds 
                    => Freight.TaxAmount - Refunds.Sum(x => x.FreightTax);

        public bool LineItemExists(string inventoryID)
        {
            return LineItem(inventoryID) != null;
        }

        public TransferLineItem LineItem(string inventoryID)
        {
            return LineItems.FirstOrDefault(x => x.InventoryID == inventoryID);
        }

        public TransferTaxCalc SplitShipmentLineItemTax(string inventoryID, int quantity)
        {
            var output = new TransferTaxCalc();
            var lineItem = LineItem(inventoryID);
            var taxableAmount = lineItem.IsTaxable ? quantity * lineItem.UnitPrice : 0m;

            output.Name = lineItem.InventoryID;
            output.TaxableAmount = taxableAmount;
            output.TaxAmount = lineItem.TaxLines.CalculateTaxes(taxableAmount);
            return output;
        }

        public TransferTaxCalc SplitShipmentFreightTax(decimal price)
        {
            var output = new TransferTaxCalc();

            output.Name = "Shipping Charges";
            output.TaxableAmount = Freight.TaxableAmount;
            output.TaxAmount = Freight.TaxLines.CalculateTaxes(Freight.TaxableAmount);
            return output;
        }
    }
}
