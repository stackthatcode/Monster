using System;
using System.Collections.Generic;
using System.Linq;
using Monster.TaxProvider.Acumatica;
using Monster.TaxProvider.InvoiceTaxes;
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

            result.AddTaxLine("Line Items", 0m, transfer.NetTotalTaxableLineAmounts, transfer.NetTotalLineItemTax);
            result.AddTaxLine("Freight", 0m, transfer.NetTotalTaxableFreight, transfer.NetTotalFreightTax);

            return result;
        }


        // InvoiceTax and InvoiceFreightTax obliterate the tax rounding issue
        //
        private CalcResult InvoiceTax(GetTaxRequest request)
        {
            var context = request.ToCalcRequestContext();
            var salesOrder = _repository.RetrieveSalesOrderByInvoice(context.InvoiceType, context.InvoiceNbr);

            var transfer = _repository.RetrieveTaxTransfer(salesOrder.OrderType, salesOrder.OrderNbr);

            _logger.Debug($"Tax Transfer - {JsonConvert.SerializeObject(transfer)}");

            var otherInvoiceTaxes =
                _invoiceTaxService
                    .GetOtherTaxes(context.InvoiceType, context.InvoiceNbr, AcumaticaTaxId.LineItemsTaxID);

            _logger.Debug($"Other Invoice Taxes - {JsonConvert.SerializeObject(otherInvoiceTaxes)}");

            if (salesOrder.OpenOrderQty == 0)
            {
                return InvoiceFinalTax(transfer, otherInvoiceTaxes);
            }
            else
            {
                return InvoiceSplitShipmentTax(request, transfer, otherInvoiceTaxes);
            }
        }

        private CalcResult InvoiceFinalTax(Transfer transfer, OtherInvoiceTaxContext otherInvoiceTaxes)
        {
            var taxableTotal =
                    transfer.NetTotalTaxableFreight + 
                    transfer.NetTotalTaxableLineAmounts -
                    otherInvoiceTaxes.TotalTaxableAmount;

            var taxTotal =
                    transfer.NetTotalLineItemTax +
                    transfer.NetTotalFreightTax -
                    otherInvoiceTaxes.TotalTaxAmount;

            var result = new CalcResult();
            result.AddTaxLine("Final Invoice Tax", 0m, taxableTotal, taxTotal);
            return result;
        }


        // Recreate Tax Calculation using the Tax Transfer
        //
        private CalcResult InvoiceSplitShipmentTax(
                GetTaxRequest request, Transfer transfer, OtherInvoiceTaxContext otherInvoiceTaxes)
        {
            var isFirstInvoice = otherInvoiceTaxes.AtLeastOneOtherInvoice;

            var result = new CalcResult();

            foreach (var lineItem in request.CartItems)
            {
                // NULL ItemCode signals that this line item is freight
                //
                if (lineItem.ItemCode == null)
                {
                    result.AddTaxLine(lineItem.Description, 0m, lineItem.Amount, transfer.NetTotalFreightTax);
                    continue;
                }

                if (transfer.LineItemExists(lineItem.ItemCode) == false)
                {
                    result.AddError($"Unable to locate Inventory ID {lineItem.ItemCode} in Tax Transfer");
                    continue;
                }

                // Compute Line Item Tax using Tax Lines from Transfer
                //
                var lineItemTaxCalc = transfer.PlainLineItemTaxCalc(lineItem.ItemCode, (int)lineItem.Quantity);

                result.AddTaxLine(lineItemTaxCalc.Name, 0m, lineItemTaxCalc.TaxableAmount, lineItemTaxCalc.TaxAmount);
            }

            return result;
        }

    }
}

