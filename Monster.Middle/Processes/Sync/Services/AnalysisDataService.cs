using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Monster.Middle.Misc.Acumatica;
using Monster.Middle.Misc.Shopify;
using Monster.Middle.Persist.Instance;
using Monster.Middle.Processes.Acumatica.Persist;
using Monster.Middle.Processes.Shopify.Persist;
using Monster.Middle.Processes.Sync.Model.Analysis;
using Push.Foundation.Utilities.Helpers;
using Push.Shopify.Api;

namespace Monster.Middle.Processes.Sync.Services
{
    public class AnalysisDataService
    {
        private readonly ProcessPersistContext _persistContext;
        private readonly ShopifyUrlService _shopifyUrlService;
        private readonly AcumaticaUrlService _acumaticaUrlService;
        private readonly OrderApi _orderApi;


        public AnalysisDataService(
                ProcessPersistContext persistContext, 
                ShopifyUrlService shopifyUrlService, 
                AcumaticaUrlService acumaticaUrlService, 
                OrderApi orderApi)
        {
            _persistContext = persistContext;
            _shopifyUrlService = shopifyUrlService;
            _acumaticaUrlService = acumaticaUrlService;
            _orderApi = orderApi;
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

        private IQueryable<ShopifyOrder> GetOrderAnalysisQueryable(OrderAnalyzerRequest request)
        {
            var queryable = _persistContext
                .Entities
                .ShopifyOrders
                .Include(x => x.AcumaticaSalesOrder)
                .Include(x => x.AcumaticaSalesOrder.AcumaticaSoShipments)
                .Include(x => x.ShopifyTransactions)
                .Include(x => x.ShopifyTransactions.Select(y => y.AcumaticaPayment))
                .Include(x => x.ShopifyRefunds)
                .Include(x => x.ShopifyFulfillments);

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


        public OrderAnalyzerDrilldown GetOrderAnalyzerDrilldown(long shopifyOrderId)
        {
            var shopifyOrder = _orderApi.Retrieve(shopifyOrderId);
            
            // Invoke Shopify for Order

            // Invoke Acumatica for Sales Order


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
