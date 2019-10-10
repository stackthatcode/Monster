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


        public TaxCalcResult CalcSplitShipmentTaxes(TaxCalcRequestFreight request)
        {
            // Acumatica idiom: the entire Freight charge is covered on the first Shipment Invoice
            //
            var result = new TaxCalcResult();
            result.TaxableAmount = request.TaxableAmount;
            result.TaxAmount = TotalFreightTaxAfterRefunds;
            return result;
        }

        public TaxCalcResult CalcSplitShipmentTaxes(TaxCalcRequestNonFreight request)
        {
            var result = new TaxCalcResult();

            foreach (var requestLineItem in request.LineItems)
            {
                var transferLineItem = this.LineItem(requestLineItem.InventoryID);
                if (transferLineItem == null)
                {
                    result.ErrorMessages.Add($"Unable to locate Inventory ID {requestLineItem.InventoryID}");
                    continue;
                }

                var taxes = transferLineItem.TaxLines.CalculateTaxes(requestLineItem.LineAmount);

                if (taxes > 0.00m)
                {
                    result.TaxableAmount += requestLineItem.LineAmount;
                    result.TaxAmount += taxes;
                }
            }

            result.Rate = 0.00m;
            return result;
        }
    }
}
