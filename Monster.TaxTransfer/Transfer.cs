using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Monster.TaxTransfer
{
    public class Transfer
    {
        public string ExternalRefNbr { get; set; }
        public List<TransferLineItem> LineItems { get; set; }
        public TransferFreight Freight { get; set; }
        public List<TransferRefund> Refunds { get; set; }

        // Non-participant in logic
        public decimal TotalPrice => LineItems.Sum(x => x.LineAmount) + Freight.Price 
                                     + LineItems.Sum(x => x.TaxAmount) + Freight.TaxAmount;
        // Non-participant in logic
        public decimal NetTotalPrice => TotalPrice
                                        - Refunds.Sum(x => x.LineItemTotal) 
                                        - Refunds.Sum(x => x.Freight);

        public decimal NetTaxableAmount =>
                    LineItems.Sum(x => x.TaxableAmount) 
                    + Freight.TaxableAmount 
                    - Refunds.Sum(x => x.TaxableAmount);
        public decimal NetLineItemTax => LineItems.Sum(x => x.TaxAmount) - Refunds.Sum(x => x.LineItemsTax);
        public decimal NetFreightTax => Freight.TaxAmount - Refunds.Sum(x => x.FreightTax);
        public decimal NetTotalTax => NetLineItemTax + NetFreightTax;


        // Non-participant in logic
        public decimal Payment { get; set; }
        // Non-participant in logic
        public decimal NetPayment => Payment -  Refunds.Sum(x => x.RefundAmount);
        // Non-participant in logic
        public decimal OverpaymentTotal => Refunds.Sum(x => x.Overpayment);


        public Transfer()
        {
            LineItems = new List<TransferLineItem>();
            Freight = new TransferFreight();
            Refunds = new List<TransferRefund>();
        }

        public bool LineItemExists(string inventoryID)
        {
            return LineItem(inventoryID) != null;
        }

        public TransferLineItem LineItem(string inventoryID)
        {
            return LineItems.FirstOrDefault(x => x.InventoryID == inventoryID);
        }

        public TransferTaxCalc PlainLineItemTaxCalc(string inventoryID, int quantity)
        {
            var output = new TransferTaxCalc();
            var lineItem = LineItem(inventoryID);
            var taxableAmount = lineItem.TaxableAmount;
                
            output.Name = $"{lineItem.InventoryID} - qty {quantity}";
            output.TaxableAmount = taxableAmount;
            output.TaxAmount = lineItem.TaxLines.CalculateTaxes(taxableAmount);
            return output;
        }
    }
}
