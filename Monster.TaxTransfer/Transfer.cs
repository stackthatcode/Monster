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


        public decimal TotalTax => LineItems.Sum(x => x.TaxAmount) + Freight.TaxAmount;
        public decimal TotalPrice => LineItems.Sum(x => x.LineAmount) + Freight.Price + TotalTax;

        public decimal NetTotalFreightTax => Freight.TaxAmount - Refunds.Sum(x => x.FreightTax);

        public decimal NetTotalTaxableAmount
                => LineItems.Sum(x => x.TaxableAmount)
                   + Freight.TaxableAmount
                   - Refunds.Sum(x => x.TotalTaxableLineAmounts)
                   - Refunds.Sum(x => x.TaxableFreightAmount);

        public decimal NetTotalTax 
                => LineItems.Sum(x => x.TaxAmount) 
                    + Freight.TaxAmount
                    - Refunds.Sum(x => x.TotalLineItemsTax)
                    - Refunds.Sum(x => x.FreightTax);

        public decimal RefundCreditTotal { get; set; }
        public decimal RefundDebitTotal { get; set; }
        public decimal NetPayment { get; set; }

        public decimal NetTotal => NetTotalTaxableAmount + NetTotalTax;

        public decimal PaymentDiscrepancy 
                => NetTotal - (NetPayment + RefundCreditTotal - RefundDebitTotal); 


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
            var taxableAmount = lineItem.IsTaxable ? quantity * lineItem.UnitPrice : 0m;

            output.Name = $"{lineItem.InventoryID} - qty {quantity}";
            output.TaxableAmount = taxableAmount;
            output.TaxAmount = lineItem.TaxLines.CalculateTaxes(taxableAmount);
            return output;
        }
    }
}
