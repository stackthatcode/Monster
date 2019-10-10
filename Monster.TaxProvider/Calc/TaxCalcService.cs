using Monster.TaxProvider.Bql;
using Monster.TaxProvider.Helpers;
using Monster.TaxTransfer;
using PX.Data;
using PX.TaxProvider;

namespace Monster.TaxProvider.Calc
{
    public class TaxCalcService
    {
        private readonly AcumaticaBqlRepository _repository;

        public TaxCalcService()
        {
            _repository = new AcumaticaBqlRepository(new PXGraph());
        }

        public TaxCalcResult CalcSalesOrderLineAmountsTax(DocContext context)
        {
            var transfer = _repository.RetrieveTaxTransfer(context.RefType, context.RefNbr);

            var result = new TaxCalcResult();
            result.TaxableAmount = transfer.TotalTaxableLineAmountsAfterRefund;
            result.Rate = 0.00m;
            result.TaxAmount = transfer.TotalLineItemTaxAfterRefunds;
            return result;
        }

        public TaxCalcResult CalcSalesOrderFreightTax(DocContext context)
        {
            var transfer = _repository.RetrieveTaxTransfer(context.RefType, context.RefNbr);

            var result = new TaxCalcResult();
            result.TaxableAmount = transfer.TotalTaxableFreightAfterRefund;
            result.Rate = 0.00m;
            result.TaxAmount = transfer.TotalFreightTaxAfterRefunds;
            return result;
        }
        

        public TaxCalcResult CalculateInvoiceTax(DocContext context)
        {
            var transfer = _repository.RetrieveTaxTransfer(context.RefType, context.RefNbr);
            var result = new TaxCalcResult();

            // TODO
            // IF THE BTP == 0, TAX = TAXAFTERREFUND - SUM(INVOICE TAXES)
            // ELSE RETURN CALC SPLIT SHIPMENT TAX
            return result;
        }

        // Attempts to simulate Tax Calculation using the loaded Rate tables
        //
        public TaxCalcResult CalcSplitShipmentTax(Transfer transfer, TaxCalcRequestNonFreight request)
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



        public TaxCalcResult CalculateInvoiceFreightTax(DocContext context)
        {
            var transfer = _repository.RetrieveTaxTransfer(context.RefType, context.RefNbr);
            var result = new TaxCalcResult();

            // TODO
            // IF THE BTP == 0, TAX = FREIGHTTAXAFTERREFUND - SUM(INVOICE FREIGHT TAXES)
            // ELSE RETURN CALC SPLIT SHIPMENT FREIGHT TAX
            return result;
        }


        public TaxCalcResult CalcSplitShipmentFreightTax(Transfer transfer, TaxCalcRequestFreight request)
        {
            // Acumatica idiom: the entire Freight charge is covered on the first Shipment Invoice
            //
            var result = new TaxCalcResult();
            result.TaxableAmount = request.TaxableAmount;
            result.Rate = 0.00m;
            result.TaxAmount = transfer.TotalFreightTaxAfterRefunds;
            return result;
        }


    }
}
