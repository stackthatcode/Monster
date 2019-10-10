using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Monster.TaxTransfer;

namespace Monster.TaxProvider
{
    public class AcumaticaTaxCalculator
    {
        public TaxCalcResult 
                CalcSplitShipmentTaxes(Transfer transfer, TaxCalcRequestFreight request)
        {
            // Acumatica idiom: the entire Freight charge is covered on the first Shipment Invoice
            //
            var result = new TaxCalcResult();
            result.TaxableAmount = request.TaxableAmount;
            result.TaxAmount = transfer.TotalFreightTaxAfterRefunds;
            return result;
        }

        public TaxCalcResult 
                CalcSplitShipmentTaxes(Transfer transfer, TaxCalcRequestNonFreight request)
        {
            var result = new TaxCalcResult();

            foreach (var requestLineItem in request.LineItems)
            {
                var transferLineItem = transfer.LineItem(requestLineItem.InventoryID);
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
