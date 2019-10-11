using System;
using System.Collections.Generic;
using System.Linq;
using Monster.TaxProvider.Bql;
using Monster.TaxProvider.Context;
using Monster.TaxTransfer;
using Newtonsoft.Json;
using PX.Data;
using PX.TaxProvider;

namespace Monster.TaxProvider.Calc
{
    public class TaxCalcService
    {
        public const string AcumaticaLineItemsTaxID = "EXTLINEITEMS";
        public const string AcumaticaFreightTaxID = "EXTFREIGHT";

        private readonly Logger _logger;
        private readonly AcumaticaBqlRepository _repository;

        public TaxCalcService(Logger logger)
        {
            _logger = logger;
            _repository = new AcumaticaBqlRepository(new PXGraph());
        }

        public GetTaxResult Calculate(GetTaxRequest request)
        {
            var context = DocContext.ExtractContext(request);
            var contextJson = JsonConvert.SerializeObject(context);

            _logger.Info($"DocContext - {contextJson}");

            if (context.DocContextType == DocContextType.SalesOrder)
            {
                return ProcessResults(SalesOrderLineAmountsTax(context));
            }

            if (context.DocContextType == DocContextType.SOFreight)
            {
                return ProcessResults(SalesOrderFreightTax(context));
            }

            if (context.DocContextType == DocContextType.SOShipmentInvoice)
            {
                return ProcessResults(InvoiceTax(request));
            }

            return BlankResult();
        }

        private GetTaxResult BlankResult()
        {
            var result = new ProviderTaxCalcResult();
            result.TaxID = AcumaticaLineItemsTaxID;
            result.TaxableAmount = 0m;
            result.Rate = 0m;
            result.TaxAmount = 0m;
            return result.ToGetTaxResult();
        }

        private GetTaxResult ProcessResults(ProviderTaxCalcResult result)
        {
            if (result.Failed)
            {
                var log = "TaxCalcService -> Calculate failed:" + Environment.NewLine;
                result.ErrorMessages.ForEach(x => log += x + Environment.NewLine);
                _logger.Info(log);

                throw new Exception("TaxCalcService -> Calculate failed");
            }
            else
            {
                return result.ToGetTaxResult();
            }
        }


        private ProviderTaxCalcResult SalesOrderLineAmountsTax(DocContext context)
        {
            var transfer = _repository.RetrieveTaxTransfer(context.RefType, context.RefNbr);

            var result = new ProviderTaxCalcResult();
            result.TaxID = AcumaticaLineItemsTaxID;
            result.TaxableAmount = transfer.TotalTaxableLineAmountsAfterRefund;
            result.Rate = 0.00m;
            result.TaxAmount = transfer.TotalLineItemTaxAfterRefunds;
            return result;
        }

        private ProviderTaxCalcResult SalesOrderFreightTax(DocContext context)
        {
            var transfer = _repository.RetrieveTaxTransfer(context.RefType, context.RefNbr);

            var result = new ProviderTaxCalcResult();
            result.TaxID = AcumaticaFreightTaxID;
            result.TaxableAmount = transfer.TotalTaxableFreightAmountAfterRefund;
            result.Rate = 0.00m;
            result.TaxAmount = transfer.TotalFreightTaxAfterRefunds;
            return result;
        }


        private ProviderTaxCalcResult InvoiceTax(GetTaxRequest request)
        {
            var context = DocContext.ExtractContext(request);
            var transfer = _repository.RetrieveTaxTransfer(context.RefType, context.RefNbr);

            // TODO - Get Sales Order by Invoice

            var result = new ProviderTaxCalcResult();

            // TODO
            // IF THE BTP == 0, TAX = TAXAFTERREFUND - SUM(INVOICE TAXES)
            // ELSE RETURN CALC SPLIT SHIPMENT TAX
            return result;
        }

        private ProviderTaxCalcResult InvoiceFreightTax(GetTaxRequest request)
        {
            throw new NotImplementedException(
                    "Which Doc Type corresponds to Invoice Freight? Haven't seen it yet!");

            //var transfer = _repository.RetrieveTaxTransfer(context.RefType, context.RefNbr);
            //var result = new ProviderTaxCalcResult();
            //// TODO
            //// IF THE BTP == 0, TAX = FREIGHTTAXAFTERREFUND - SUM(INVOICE FREIGHT TAXES)
            //// ELSE RETURN CALC SPLIT SHIPMENT FREIGHT TAX
            //return result;
        }


        // Recreate Tax Calculation using the Tax Transfer
        //
        private ProviderTaxCalcResult InvoiceSplitShipmentTax(GetTaxRequest request)
        {
            var context = DocContext.ExtractContext(request);
            var transfer = _repository.RetrieveTaxTransfer(context.RefType, context.RefNbr);

            var result = new ProviderTaxCalcResult();
            var transferTaxes = new List<TransferTaxCalc>();

            foreach (var lineItem in request.CartItems)
            {
                if (transfer.LineItemExists(lineItem.ItemCode) == false)
                {
                    result.ErrorMessages.Add(
                        $"Unable to locate Inventory ID {lineItem.ItemCode} in Tax Transfer");
                    continue;
                }

                var transferTax = transfer.SplitShipmentLineItemTax(lineItem.ItemCode, (int)lineItem.Quantity);

                transferTaxes.Add(transferTax);
            }

            result.TaxableAmount = transferTaxes.Sum(x => x.TaxableAmount);
            result.TaxAmount = transferTaxes.Sum(x => x.TaxAmount);
            result.Rate = 0.00m;
            return result;
        }

        private ProviderTaxCalcResult InvoiceSplitShipmentFreightTax(GetTaxRequest request)
        {
            var context = DocContext.ExtractContext(request);
            var transfer = _repository.RetrieveTaxTransfer(context.RefType, context.RefNbr);

            // Acumatica idiom: the entire Freight charge is covered on the first Shipment Invoice
            //
            var cartItem = request.CartItems.First();
            var result = new ProviderTaxCalcResult();
            result.TaxableAmount = cartItem.Amount;
            result.Rate = 0.00m;
            result.TaxAmount = transfer.TotalFreightTaxAfterRefunds;
            return result;
        }

    }
}
