using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Monster.Acumatica.Api;
using Monster.Acumatica.Api.SalesOrder;
using Monster.Acumatica.Http;
using Monster.Middle.Misc.Acumatica;
using Monster.Middle.Misc.Shopify;
using Monster.Middle.Persist.Instance;
using Monster.Middle.Processes.Acumatica.Persist;
using Monster.Middle.Processes.Shopify.Persist;
using Monster.Middle.Processes.Sync.Model.Analysis;
using Monster.Middle.Processes.Sync.Model.Orders;
using Push.Foundation.Utilities.Helpers;

namespace Monster.Middle.Processes.Sync.Services
{
    public class AnalysisDataService
    {
        private readonly ProcessPersistContext _persistContext;
        private readonly ShopifyUrlService _shopifyUrlService;
        private readonly AcumaticaUrlService _acumaticaUrlService;
        private readonly AcumaticaHttpContext _acumaticaHttpContext;
        private readonly SalesOrderClient _salesOrderClient;


        public AnalysisDataService(
                ProcessPersistContext persistContext, 
                ShopifyUrlService shopifyUrlService, 
                AcumaticaUrlService acumaticaUrlService, 
                AcumaticaHttpContext acumaticaHttpContext)
        {
            _persistContext = persistContext;
            _shopifyUrlService = shopifyUrlService;
            _acumaticaUrlService = acumaticaUrlService;
            _acumaticaHttpContext = acumaticaHttpContext;
        }


        public int GetOrderAnalysisRecordCount(OrderAnalyzerRequest request)
        {
            return GetOrderAnalysisQueryable(request).Count();
        }


        public List<OrderAnalyzerResultsRow> GetOrderAnalysisResults(OrderAnalyzerRequest request)
        {
            var queryable = GetOrderAnalysisQueryable(request);   
            var results = queryable
                .OrderByDescending(x => x.ShopifyOrderId)
                .Skip(request.StartRecord)
                .Take(request.PageSize)
                .ToList();

            return results.Select(x => Make(x)).ToList();
        }

        private IQueryable<ShopifyOrder> ShopifyOrderQueryable =>
            _persistContext
                .Entities
                .ShopifyOrders
                .Include(x => x.AcumaticaSalesOrder)
                .Include(x => x.AcumaticaSalesOrder.AcumaticaSoShipments)
                .Include(x => x.ShopifyTransactions)
                .Include(x => x.ShopifyTransactions.Select(y => y.AcumaticaPayment))
                .Include(x => x.ShopifyRefunds)
                .Include(x => x.ShopifyFulfillments);

        private IQueryable<ShopifyOrder> GetOrderAnalysisQueryable(OrderAnalyzerRequest request)
        {
            var queryable = ShopifyOrderQueryable;

            foreach (var term in ParseSearchTerms(request.SearchText))
            {
                if (term.IsLong())
                {
                    queryable = queryable.Where(
                        x => x.ShopifyOrderId == term.ToLong() ||
                             x.AcumaticaSalesOrder.AcumaticaOrderNbr.Contains(term));
                }
                else
                {
                    queryable = queryable.Where(
                        x => x.ShopifyOrderNumber.Contains(term) ||
                             x.AcumaticaSalesOrder.AcumaticaOrderNbr.Contains(term));
                }
            }

            return queryable;
        }


        public OrderAnalysisTotals GetOrderTotals(long shopifyOrderId)
        {
            var shopifyOrderRecord = ShopifyOrderQueryable.FirstOrDefault(x => x.ShopifyOrderId == shopifyOrderId);
            var shopifyOrder = shopifyOrderRecord.ToShopifyObj();

            var output = new OrderAnalysisTotals();

            output.ShopifyOrderNbr = shopifyOrderRecord.ShopifyOrderNumber;
            output.ShopifyOrderHref = _shopifyUrlService.ShopifyOrderUrl(shopifyOrderRecord.ShopifyOrderId);
            output.ShopifyShippingPriceTotal = shopifyOrder.ShippingDiscountedTotal.AnalysisFormat();
            output.ShopifyTotalTax = shopifyOrder.total_tax.AnalysisFormat();
            output.ShopifyOrderTotal = shopifyOrder.total_price.AnalysisFormat();

            output.ShopifyOrderPayment 
                = shopifyOrderRecord.PaymentTransaction().ShopifyAmount.AnalysisFormat();
            output.ShopifyRefundPayment 
                = shopifyOrderRecord.RefundTransactions().Sum(x => x.ShopifyAmount).AnalysisFormat();
            output.ShopifyNetPayment
                = shopifyOrderRecord.ShopifyNetPayment().AnalysisFormat();

            output.ShopifyRefundItemTotal = shopifyOrder.RefundLineItemTotal.AnalysisFormat();
            output.ShopifyRefundShippingTotal = shopifyOrder.RefundShippingTotal.AnalysisFormat();
            output.ShopifyRefundTaxTotal = shopifyOrder.RefundTotalTax.AnalysisFormat();
            output.ShopifyCreditTotal = shopifyOrder.RefundCreditTotal.AnalysisFormat();
            output.ShopifyDebitTotal = shopifyOrder.RefundDebitTotal.AnalysisFormat();
            output.ShopifyRefundTotal = shopifyOrder.RefundTotal.AnalysisFormat();

            if (shopifyOrderRecord.IsSynced())
            {
                output.AcumaticaSalesOrderNbr = shopifyOrderRecord.AcumaticaSalesOrder.AcumaticaOrderNbr;
                output.AcumaticaSalesOrderHref
                    = _acumaticaUrlService.AcumaticaSalesOrderUrl(
                            SalesOrderType.SO, shopifyOrderRecord.AcumaticaSalesOrder.AcumaticaOrderNbr);

                InjectTotalsFromAcumatica(shopifyOrderRecord, output);

                output.AcumaticaPaymentTotal = shopifyOrderRecord.AcumaticaPaymentAmount().AnalysisFormat();
                output.AcumaticaRefundPaymentTotal =
                    shopifyOrderRecord.AcumaticaCustomerRefundTotal().AnalysisFormat();
                output.AcumaticaNetPaymentTotal = shopifyOrderRecord.AcumaticaNetPaymentAmount().AnalysisFormat();

                //output.AcumaticaCreditTotal
                //output.AcumaticaRefundDebitTotal
                //output.AcumaticaCreditDebitMemoTotal

                output.AcumaticaInvoiceTaxTotal = shopifyOrderRecord.AcumaticaInvoiceTaxTotal().AnalysisFormat();
                output.AcumaticaInvoiceTotal = shopifyOrderRecord.AcumaticaInvoiceTotal().AnalysisFormat();
            }

            return output;
        }

        private void InjectTotalsFromAcumatica(ShopifyOrder shopifyOrderRecord, OrderAnalysisTotals output)
        {
            SalesOrder acumaticaOrder = null;
            var acumaticaOrderNbr = shopifyOrderRecord.AcumaticaSalesOrder.AcumaticaOrderNbr;

            _acumaticaHttpContext.SessionRun(() =>
            {
                var json = _salesOrderClient
                    .RetrieveSalesOrder(acumaticaOrderNbr, SalesOrderType.SO, Expand.Totals);
                acumaticaOrder = json.ToSalesOrderObj();
            });

            output.AcumaticaOrderLineTotal = acumaticaOrder.Totals.LineTotalAmount.value.AnalysisFormat();
            output.AcumaticaOrderFreight = acumaticaOrder.Totals.Freight.value.AnalysisFormat();
            output.AcumaticaTaxTotal = acumaticaOrder.Totals.TaxTotal.value.AnalysisFormat();
            output.AcumaticaOrderTotal = acumaticaOrder.OrderTotal.value.AnalysisFormat();
        }


        private OrderAnalyzerResultsRow Make(ShopifyOrder order)
        {
            var output = new OrderAnalyzerResultsRow();
            output.ShopifyOrderNbr = order.ShopifyOrderNumber.ToString();
            output.ShopifyOrderHref = _shopifyUrlService.ShopifyOrderUrl(order.ShopifyOrderId);

            var shopifyOrder = order.ToShopifyObj();
            output.ShopifyOrderTotal = shopifyOrder.total_price.AnalysisFormat();

            output.ShopifyNetPayment = order.ShopifyNetPayment().AnalysisFormat();

            if (order.AcumaticaSalesOrder != null)
            {
                output.AcumaticaSalesOrderNbr = order.AcumaticaSalesOrder.AcumaticaOrderNbr;
                output.AcumaticaSalesOrderHref =
                    _acumaticaUrlService.AcumaticaSalesOrderUrl(
                            SalesOrderType.SO, order.AcumaticaSalesOrder.AcumaticaOrderNbr);

                if (order.IsPaymentSynced())
                {
                    output.AcumaticaOrderPayment = order.AcumaticaPaymentAmount().AnalysisFormat();
                    output.AcumaticaNetPayment = order.AcumaticaNetPaymentAmount().AnalysisFormat();
                }
                else
                {
                    output.AcumaticaOrderPayment = 0.00m.AnalysisFormat();
                    output.AcumaticaNetPayment = 0.00m.AnalysisFormat();
                }

                output.AcumaticaInvoiceTotal = order.AcumaticaInvoiceTotal().AnalysisFormat();
            }

            return output;
        }

        private List<string> ParseSearchTerms(string rawInput)
        {
            var output = new List<string>();

            if (rawInput.IsNullOrEmpty())
            {
                return output;
            }
            else
            {
                return rawInput.SplitBy(' ').Where(x => !x.Trim().IsNullOrEmpty()).ToList();
            }
        }
    }
}
