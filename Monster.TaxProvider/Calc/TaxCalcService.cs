using System;
using System.Collections.Generic;
using System.Linq;
using Monster.TaxProvider.Acumatica;
using Monster.TaxProvider.InvoiceTaxService;
using Monster.TaxProvider.Utility;
using Monster.TaxTransfer;
using Newtonsoft.Json;
using PX.Data;
using PX.TaxProvider;


namespace Monster.TaxProvider.Calc
{
    public class TaxCalcService
    {
        private readonly Logger _logger;
        private readonly BqlRepository _repository;
        private readonly OtherInvoiceTaxService _invoiceTaxService;

        public TaxCalcService(Logger logger)
        {
            _logger = logger;
            _repository = new BqlRepository(new PXGraph());
            _invoiceTaxService = new OtherInvoiceTaxService(_repository);
        }

        public GetTaxResult Calculate(GetTaxRequest request)
        {
            ProviderContext context = 

            if (context.DocContextType == ProviderContextType.SalesOrder)
            {
                return ProcessResults(SalesOrderLineAmountsTax(context));
            }

            if (context.DocContextType == ProviderContextType.SOFreight)
            {
                return ProcessResults(SalesOrderFreightTax(context));
            }

            if (context.DocContextType == ProviderContextType.SOShipmentInvoice)
            {
                return ProcessResults(InvoiceTax(context));
            }

            return new ProviderTaxCalcResult();
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


        private ProviderTaxCalcResult SalesOrderLineAmountsTax(ProviderContext context)
        {
            var transfer = _repository.RetrieveTaxTransfer(context.RefType, context.RefNbr);

            var result = new ProviderTaxCalcResult();
            result.TaxID = AcumaticaLineItemsTaxID;
            result.TaxableAmount = transfer.TotalTaxableLineAmountsAfterRefund;
            result.Rate = 0.00m;
            result.TaxAmount = transfer.TotalLineItemTaxAfterRefunds;
            return result;
        }

        private ProviderTaxCalcResult SalesOrderFreightTax(ProviderContext context)
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
        private ProviderTaxCalcResult InvoiceTax(ProviderContext context)
        {
            var salesOrder = _repository.RetrieveSalesOrderByInvoice(context.RefType, context.RefNbr);

            if (salesOrder.OpenOrderQty == 0)
            {
                var transfer = _repository.RetrieveTaxTransfer(salesOrder.OrderType, salesOrder.OrderNbr);

                var otherInvoiceTaxes = 
                    _invoiceTaxService.GetOtherTaxesSummary(
                            context.RefType, context.RefNbr, AcumaticaTaxIdentifiers.LineItemsTaxID);

                var arTransJson = JsonConvert.SerializeObject(otherInvoiceTaxes);
                _logger.Info($"Other Invoice Taxes - {arTransJson}");

                var taxableAmount =
                    transfer.TotalTaxableLineAmountsAfterRefund - otherInvoiceTaxes.TotalTaxableAmount;

                var taxAmount = transfer.TotalLineItemTaxAfterRefunds - otherInvoiceTaxes.TotalTaxAmount;

                return ProviderTaxCalcResult
                        .Make(AcumaticaTaxIdentifiers.LineItemsTaxID, taxableAmount, taxAmount, 0m);
            }
            else
            {
                return InvoiceSplitShipmentTax(context);
            }
        }

        private ProviderTaxCalcResult InvoiceFreightTax(ProviderContext context)
        {
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



        // Recreate Tax Calculation using the Tax Transfer
        //
        private ProviderTaxCalcResult InvoiceSplitShipmentTax(ProviderContext context)
        {
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
            return result;
        }

        // Acumatica idioms - the entire Freight charge is covered on the first Shipment Invoice
        //
        private ProviderTaxCalcResult InvoiceSplitShipmentFreightTax(GetTaxRequest request)
        {
            var context = ProviderContext.ExtractContext(request);
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

