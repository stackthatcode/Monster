using System;
using System.Collections.Generic;
using System.Linq;
using Monster.TaxProvider.Bql;
using Monster.TaxProvider.Context;
using Monster.TaxProvider.Utility;
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

            // TODO - where is the Invoice Freight Tax computed...?

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


        // InvoiceTax and InvoiceFreightTax obliterate the tax rounding issue
        //
        private ProviderTaxCalcResult InvoiceTax(GetTaxRequest request)
        {
            var context = DocContext.ExtractContext(request);

            var salesOrder = _repository.RetrieveSalesOrderByInvoice(context.RefType, context.RefNbr);

            if (salesOrder.OpenOrderQty == 0)
            {
                var transfer = _repository.RetrieveTaxTransfer(salesOrder.OrderType, salesOrder.OrderNbr);
                var arTrans = BuildTaxTranDigest(context, AcumaticaLineItemsTaxID);
                var arTransJson = JsonConvert.SerializeObject(arTrans);

                _logger.Info($"ARTrans Digest - {arTransJson}");

                var taxableAmount = 
                    transfer.TotalTaxableLineAmountsAfterRefund - arTrans.Sum(x => x.TaxableAmount);

                var taxAmount = 
                    transfer.TotalLineItemTaxAfterRefunds - arTrans.Sum(x => x.TaxAmount);

                return ProviderTaxCalcResult.Make(AcumaticaLineItemsTaxID, taxableAmount, taxAmount, 0m);
            }
            else
            {
                return InvoiceSplitShipmentTax(request);
            }
        }

        private ProviderTaxCalcResult InvoiceFreightTax(GetTaxRequest request)
        {
            var context = DocContext.ExtractContext(request);

            var salesOrder = _repository.RetrieveSalesOrderByInvoice(context.RefType, context.RefNbr);

            if (salesOrder.OpenOrderQty == 0)
            {
                var transfer = _repository.RetrieveTaxTransfer(salesOrder.OrderType, salesOrder.OrderNbr);
                var arTrans = BuildTaxTranDigest(context, AcumaticaFreightTaxID);
                var arTransJson = JsonConvert.SerializeObject(arTrans);

                _logger.Info($"ARTrans Digest - {arTransJson}");

                var taxableAmount = 
                    transfer.TotalTaxableFreightAmountAfterRefund - arTrans.Sum(x => x.TaxableAmount);

                var taxAmount = 
                    transfer.TotalFreightTaxAfterRefunds - arTrans.Sum(x => x.TaxAmount);

                return ProviderTaxCalcResult.Make(AcumaticaFreightTaxID, taxableAmount, taxAmount, 0m);
            }
            else
            {
                return InvoiceSplitShipmentTax(request);
            }
        }


        private IList<ARTaxTranDigest> BuildTaxTranDigest(DocContext context, string taxId)
        {
            var salesOrder =
                _repository.RetrieveSalesOrderByInvoice(context.RefType, context.RefNbr);

            var arTrans =
                _repository
                    .RetrieveARTaxTransactions(salesOrder.OrderType, salesOrder.OrderNbr, taxId)
                    .ToDigests()
                    .ExcludeInvoice(context.RefType, context.RefNbr);
            return arTrans;
        }


        // Recreate Tax Calculation using the Tax Transfer
        //
        private ProviderTaxCalcResult InvoiceSplitShipmentTax(GetTaxRequest request)
        {
            var context = DocContext.ExtractContext(request);

            // Calculate Taxes using Transfer
            //
            var transfer = _repository.RetrieveTaxTransfer(context.RefType, context.RefNbr);
            var transferTaxes = new List<TransferTaxCalc>();
            var messages = new List<string>();

            foreach (var lineItem in request.CartItems)
            {
                if (transfer.LineItemExists(lineItem.ItemCode) == false)
                {
                    messages.Add($"Unable to locate Inventory ID {lineItem.ItemCode} in Tax Transfer");
                    continue;
                }

                var transferTax = transfer.SplitShipmentLineItemTax(lineItem.ItemCode, (int)lineItem.Quantity);
                transferTaxes.Add(transferTax);
            }

            var json = JsonConvert.SerializeObject(transferTaxes);
            _logger.Info("Transfer - Split Shipment Tax - " + json);

            // Translate from bounded context 
            //
            var result = new ProviderTaxCalcResult();
            result.ErrorMessages = messages;
            result.TaxableAmount = transferTaxes.Sum(x => x.TaxableAmount);
            result.TaxAmount = transferTaxes.Sum(x => x.TaxAmount);
            result.Rate = 0.00m;
            return result;
        }

        // Acumatica idioms - the entire Freight charge is covered on the first Shipment Invoice
        //
        private ProviderTaxCalcResult InvoiceSplitShipmentFreightTax(GetTaxRequest request)
        {
            var context = DocContext.ExtractContext(request);
            var transfer = _repository.RetrieveTaxTransfer(context.RefType, context.RefNbr);

            var cartItem = request.CartItems.First();
            var result = new ProviderTaxCalcResult();
            result.TaxableAmount = cartItem.Amount;
            result.Rate = 0.00m;
            result.TaxAmount = transfer.TotalFreightTaxAfterRefunds;
            return result;
        }
    }
}

