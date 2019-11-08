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
            var calcRequestType = request.ToCalcRequestType();
            _logger.Info($"CalcRequestType - {JsonConvert.SerializeObject(calcRequestType)}");

            if (calcRequestType.Type == CalcRequestTypeEnum.SalesOrder)
            {
                return ProcessResults(SalesOrderTax(calcRequestType));
            }

            if (calcRequestType.Type == CalcRequestTypeEnum.SOShipmentInvoice)
            {
                return ProcessResults(InvoiceTax(calcRequestType));
            }

            return ProcessResults(new CalcResult());
        }

        private GetTaxResult ProcessResults(CalcResult result)
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
                var json = JsonConvert.SerializeObject(result);
                _logger.Info($"Calc Result - {json}");
                return result.ToGetTaxResult();
            }
        }


        private CalcResult SalesOrderTax(CalcRequestType context)
        {
            var transfer = _repository.RetrieveTaxTransfer(context.RefType, context.RefNbr);

            var result = new CalcResult();

            result.Add(new CalcResultTaxLine(
                "Line Items", 
                0m, 
                transfer.TotalTaxableLineAmountsAfterRefund,
                transfer.TotalLineItemTaxAfterRefunds));

            result.Add(new CalcResultTaxLine(
                "Freight",
                0m,
                transfer.TotalTaxableFreightAmountAfterRefund,
                transfer.TotalFreightTaxAfterRefunds));

            return result;
        }



        // InvoiceTax and InvoiceFreightTax obliterate the tax rounding issue
        //
        private CalcResult InvoiceTax(CalcRequestType context)
        {
            var salesOrder = _repository.RetrieveSalesOrderByInvoice(context.RefType, context.RefNbr);

            if (salesOrder.OpenOrderQty == 0)
            {
                var transfer = _repository.RetrieveTaxTransfer(salesOrder.OrderType, salesOrder.OrderNbr);

                var otherInvoiceTaxes = 
                    _invoiceTaxService.GetOtherTaxes(
                            context.RefType, context.RefNbr, AcumaticaTaxIdentifiers.LineItemsTaxID);

                var arTransJson = JsonConvert.SerializeObject(otherInvoiceTaxes);
                _logger.Info($"Other Invoice Taxes - {arTransJson}");

                var taxableAmount =
                    transfer.TotalTaxableLineAmountsAfterRefund - otherInvoiceTaxes.TotalTaxableAmount;

                var taxAmount = transfer.TotalLineItemTaxAfterRefunds - otherInvoiceTaxes.TotalTaxAmount;

                return CalcResult
                        .Make(AcumaticaTaxIdentifiers.LineItemsTaxID, taxableAmount, taxAmount, 0m);
            }
            else
            {
                return InvoiceSplitShipmentTax(context);
            }
        }

        private CalcResult InvoiceFreightTax(CalcRequestType context)
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

                return CalcResult.Make(AcumaticaFreightTaxID, taxableAmount, taxAmount, 0m);
            }
            else
            {
                return InvoiceSplitShipmentTax(request);
            }
        }


        // Recreate Tax Calculation using the Tax Transfer
        //
        private CalcResult InvoiceSplitShipmentTax(CalcRequestType context)
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
            var result = new CalcResult();
            result.ErrorMessages = messages;
            return result;
        }

    }
}

