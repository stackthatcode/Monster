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
                AcumaticaHttpContext acumaticaHttpContext, 
                SalesOrderClient salesOrderClient)
        {
            _persistContext = persistContext;
            _shopifyUrlService = shopifyUrlService;
            _acumaticaUrlService = acumaticaUrlService;
            _acumaticaHttpContext = acumaticaHttpContext;
            _salesOrderClient = salesOrderClient;
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
                    var shopifyOrderId = term.ToLong();
                    queryable = queryable.Where(
                        x => x.ShopifyOrderId == shopifyOrderId ||
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
            output.ShopifyOrderId = shopifyOrderRecord.ShopifyOrderId;
            output.ShopifyOrderHref = _shopifyUrlService.ShopifyOrderUrl(shopifyOrderRecord.ShopifyOrderId);
            output.ShopifyTotalLinePrice = shopifyOrder.total_line_items_price;
            output.ShopifyShippingPriceTotal = shopifyOrder.ShippingDiscountedTotal;
            output.ShopifyTotalTax = shopifyOrder.total_tax;
            output.ShopifyOrderTotal = shopifyOrder.total_price;

            output.ShopifyOrderPayment
                = shopifyOrderRecord.ShopifyPaymentAmount();
            output.ShopifyRefundPayment 
                = shopifyOrderRecord.RefundTransactions().Sum(x => x.ShopifyAmount);
            output.ShopifyNetPayment
                = shopifyOrderRecord.ShopifyNetPayment();

            output.ShopifyRefundItemTotal = shopifyOrder.RefundLineItemTotal;
            output.ShopifyRefundShippingTotal = shopifyOrder.RefundShippingTotal;
            output.ShopifyRefundTaxTotal = shopifyOrder.RefundTotalTax;
            output.ShopifyCreditTotal = shopifyOrder.RefundCreditTotal;
            output.ShopifyDebitTotal = shopifyOrder.RefundDebitTotal;
            output.ShopifyRefundTotal = shopifyOrder.RefundTotal;
            output.ShopifyRefundDiscrepancyTotal = shopifyOrder.RefundDiscrepancyTotal;

            if (shopifyOrderRecord.IsSynced())
            {
                output.AcumaticaSalesOrderNbr = shopifyOrderRecord.AcumaticaSalesOrder.AcumaticaOrderNbr;
                output.AcumaticaSalesOrderHref
                    = _acumaticaUrlService.AcumaticaSalesOrderUrl(
                            SalesOrderType.SO, shopifyOrderRecord.AcumaticaSalesOrder.AcumaticaOrderNbr);

                InjectTotalsFromAcumatica(shopifyOrderRecord, output);

                output.AcumaticaPaymentTotal = shopifyOrderRecord.AcumaticaPaymentAmount();
                output.AcumaticaRefundPaymentTotal = shopifyOrderRecord.AcumaticaCustomerRefundTotal();
                output.AcumaticaNetPaymentTotal = shopifyOrderRecord.AcumaticaNetPaymentAmount();

                //output.AcumaticaCreditTotal
                //output.AcumaticaRefundDebitTotal
                //output.AcumaticaCreditDebitMemoTotal

                output.AcumaticaInvoiceTaxTotal = shopifyOrderRecord.AcumaticaInvoiceTaxTotal();
                output.AcumaticaInvoiceTotal = shopifyOrderRecord.AcumaticaInvoiceTotal();
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

            output.AcumaticaOrderLineTotal = (decimal)acumaticaOrder.Totals.LineTotalAmount.value;
            output.AcumaticaOrderFreight = (decimal)acumaticaOrder.Totals.Freight.value;
            output.AcumaticaTaxTotal = (decimal)acumaticaOrder.Totals.TaxTotal.value;
            output.AcumaticaOrderTotal = (decimal)acumaticaOrder.OrderTotal.value;
        }


        private OrderAnalyzerResultsRow Make(ShopifyOrder order)
        {
            var output = new OrderAnalyzerResultsRow();
            output.ShopifyOrderId = order.ShopifyOrderId;
            output.ShopifyOrderNbr = order.ShopifyOrderNumber.ToString();
            output.ShopifyOrderHref = _shopifyUrlService.ShopifyOrderUrl(order.ShopifyOrderId);

            var shopifyOrder = order.ToShopifyObj();
            output.ShopifyOrderTotal = shopifyOrder.total_price;

            output.ShopifyNetPayment = order.ShopifyNetPayment();

            if (order.AcumaticaSalesOrder != null)
            {
                output.AcumaticaSalesOrderNbr = order.AcumaticaSalesOrder.AcumaticaOrderNbr;
                output.AcumaticaSalesOrderHref =
                    _acumaticaUrlService.AcumaticaSalesOrderUrl(
                            SalesOrderType.SO, order.AcumaticaSalesOrder.AcumaticaOrderNbr);

                if (order.IsPaymentSynced())
                {
                    output.AcumaticaOrderPayment = order.AcumaticaPaymentAmount();
                    output.AcumaticaNetPayment = order.AcumaticaNetPaymentAmount();
                }
                else
                {
                    output.AcumaticaOrderPayment = 0.00m;
                    output.AcumaticaNetPayment = 0.00m;
                }

                output.AcumaticaInvoiceTotal = order.AcumaticaInvoiceTotal();
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
