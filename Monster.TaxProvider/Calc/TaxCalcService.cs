using System;
using System.Collections.Generic;
using System.Linq;
using Monster.TaxProvider.Acumatica;
using Monster.TaxProvider.InvoiceTaxService;
using Monster.TaxProvider.Utility;
using Monster.TaxTransfer;
using Newtonsoft.Json;
using PX.Data;
using PX.Objects.SO;
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
            var calcRequestType = request.ToCalcRequestContext();
            _logger.Info($"CalcRequestType - {JsonConvert.SerializeObject(calcRequestType)}");

            if (calcRequestType.Type == CalcRequestTypeEnum.SalesOrder)
            {
                return ProcessResults(SalesOrderTax(calcRequestType));
            }

            if (calcRequestType.Type == CalcRequestTypeEnum.SOShipmentInvoice)
            {
                return ProcessResults(InvoiceTax(request));
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

        private CalcResult SalesOrderTax(CalcRequestContext context)
        {
            var transfer = _repository.RetrieveTaxTransfer(context.OrderType, context.OrderNbr);
            var result = new CalcResult();

            result.Add(new CalcResultTaxLine(
                "Line Items", 
                0m, 
                transfer.NetTotalTaxableLineAmounts,
                transfer.NetTotalLineItemTax));

            result.Add(new CalcResultTaxLine(
                "Freight",
                0m,
                transfer.NetTotalTaxableFreight,
                transfer.NetTotalFreightTax));

            return result;
        }


        // InvoiceTax and InvoiceFreightTax obliterate the tax rounding issue
        //
        private CalcResult InvoiceTax(GetTaxRequest request)
        {
            var context = request.ToCalcRequestContext();
            var salesOrder = _repository.RetrieveSalesOrderByInvoice(context.InvoiceType, context.InvoiceNbr);

            if (salesOrder.OpenOrderQty == 0)
            {
                return InvoiceFinalTax(context, salesOrder);
            }
            else
            {
                return InvoiceSplitShipmentTax(request, otherInvoiceTaxes);
            }
        }

        private CalcResult InvoiceFinalTax(CalcRequestContext context, SOOrder salesOrder)
        {
            var transfer = _repository.RetrieveTaxTransfer(salesOrder.OrderType, salesOrder.OrderNbr);

            var otherInvoiceTaxes =
                _invoiceTaxService
                    .GetOtherTaxes(context.InvoiceType, context.InvoiceNbr, AcumaticaTaxId.LineItemsTaxID);

            var taxableTotal =
                    transfer.NetTotalTaxableFreight + 
                    transfer.NetTotalTaxableLineAmounts -
                    otherInvoiceTaxes.TotalTaxableAmount;

            var taxTotal =
                    transfer.NetTotalLineItemTax +
                    transfer.NetTotalFreightTax -
                    otherInvoiceTaxes.TotalTaxAmount;

            var result = new CalcResult();
            result.Add(new CalcResultTaxLine("Final Invoice Tax", 0m, taxableTotal, taxTotal));
            return result;
        }

        private CalcResult InvoiceFreightTax(CalcRequestContext context, OtherInvoiceTaxService otherInvoiceTax)
        {

            if (salesOrder.OpenOrderQty == 0)
            {
                var arTrans = BuildTaxTranDigest(context, AcumaticaFreightTaxID);
                var arTransJson = JsonConvert.SerializeObject(arTrans);

                _logger.Info($"ARTrans Digest - {arTransJson}");

                var taxableAmount = 
                    transfer.TotalTaxableFreightAfterRefund - arTrans.Sum(x => x.TaxableAmount);

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
        private CalcResult InvoiceSplitShipmentTax(CalcRequestContext context)
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

