using System;
using System.Collections.Generic;
using System.Linq;
using Monster.TaxProvider.Acumatica;
using Monster.TaxProvider.InvoiceTaxes;
using Monster.TaxProvider.InvoiceTaxService;
using Monster.TaxProvider.Utility;
using Monster.TaxTransfer.v2;
using Newtonsoft.Json;
using PX.Data;
using PX.TaxProvider;


namespace Monster.TaxProvider.Calc
{
    public class TaxCalcService
    {
        private readonly Logger _logger;
        private readonly List<ITaxProviderSetting> _settings;

        public const string CalcVersion = "2020.02.22.3";

        public TaxCalcService(Logger logger, List<ITaxProviderSetting> settings)
        {
            _logger = logger;
            _settings = settings;
        }

        public GetTaxResult Calculate(GetTaxRequest request)
        {
            var calcRequestType = request.ToCalcRequestContext();
            _logger.Info(
                $"TaxCalcService {CalcVersion} -> Calculate (Type) - " +
                $"{JsonConvert.SerializeObject(calcRequestType)}");

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
                var log = $"TaxCalcService {CalcVersion} -> Calculate failed:" + Environment.NewLine;
                result.ErrorMessages.ForEach(x => log += x + Environment.NewLine);

                _logger.Info(log);

                throw new Exception($"TaxCalcService {CalcVersion} -> Calculate failed");
            }
            else
            {
                var acumaticaTaxId = _settings.Setting(ProviderSettings.SETTING_EXTERNALTAXID).Value;
                var json = JsonConvert.SerializeObject(result);
                _logger.Info($"TaxCalcService {CalcVersion} -> Calculate (Result) - {json}");

                var output = result.ToGetTaxResult(acumaticaTaxId);
                return output;
            }
        }

        private CalcResult SalesOrderTax(CalcRequestContext context)
        {
            var repository = new BqlRepository(new PXGraph(), _logger);

            var transfer = repository.RetrieveTaxTransfer(context.OrderType, context.OrderNbr);
            var result = new CalcResult();

            result.AddTaxLine("Sales Order Tax", 0m, transfer.NetTaxableAmount, transfer.NetTotalTax);
            return result;
        }



        private CalcResult InvoiceTax(GetTaxRequest request)
        {
            var context = request.ToCalcRequestContext();

            var graph = new PXGraph();
            var repository = new BqlRepository(graph, _logger);
            var invoiceTaxService = new OtherInvoiceTaxService(repository, _logger);

            var salesOrder = repository.RetrieveSalesOrderByInvoice(context.InvoiceType, context.InvoiceNbr);
            var transfer = repository.RetrieveTaxTransfer(salesOrder.OrderType, salesOrder.OrderNbr);

            var otherInvoiceTaxes = invoiceTaxService.GetOtherTaxes(context.InvoiceType, context.InvoiceNbr);

            // *** TODO - get the Sales Order Shipments and count Shipped Qty therefrom
            //      ... as means to protect against multiple non-Invoiced Shipments causing OpenOrderQty == 0
            //
            if (salesOrder.OpenOrderQty == 0)
            {
                return InvoiceFinalTax(transfer, otherInvoiceTaxes);
            }
            else
            {
                return InvoiceSplitShipmentTax(request, transfer);
            }
        }

        private CalcResult InvoiceFinalTax(TaxTransfer.v2.TaxTransfer transfer, OtherInvoiceTaxContext otherInvoiceTaxes)
        {
            var taxableTotal = transfer.NetTaxableAmount - otherInvoiceTaxes.TotalTaxableAmount;
            var taxTotal = transfer.NetTotalTax - otherInvoiceTaxes.TotalTaxAmount;

            // *** Add Payment Discrepancy
            //
            var result = new CalcResult();
            result.AddTaxLine("Final Invoice Tax", 0m, taxableTotal, taxTotal);
            return result;
        }


        // Recreate Tax Calculation using the Tax Transfer
        //
        private CalcResult InvoiceSplitShipmentTax(GetTaxRequest request, TaxTransfer.v2.TaxTransfer snapshot)
        {
            var result = new CalcResult();

            foreach (var lineItem in request.CartItems)
            {
                // NULL ItemCode signals that this line item is freight
                //
                if (lineItem.ItemCode == null)
                {
                    if (lineItem.Amount > 0.00m)
                    {
                        result.AddTaxLine(lineItem.Description, 0m, lineItem.Amount, snapshot.NetFreightTax);
                    }

                    continue;
                }

                var correctedItemCode = lineItem.ItemCode.Trim();

                if (snapshot.LineItems.Any(x => x.ItemID == correctedItemCode) == false)
                {
                    result.AddError($"Unable to locate Inventory ID {correctedItemCode} in Tax Transfer");
                    continue;
                }

                // Compute Line Item Tax using Tax Lines from Transfer
                //
                var lineItemTaxCalc 
                    = snapshot.CalculateTax(correctedItemCode, (decimal)lineItem.Amount, (int)lineItem.Quantity);

                result.AddTaxLine(lineItemTaxCalc.Name, 0m, lineItemTaxCalc.TaxableAmount, lineItemTaxCalc.TaxAmount);
            }

            return result;
        }
    }
}

