using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Monster.Acumatica.Api;
using Monster.Acumatica.Api.Distribution;
using Monster.Acumatica.Api.SalesOrder;
using Monster.Acumatica.Http;
using Monster.Middle.Misc.Acumatica;
using Monster.Middle.Misc.Shopify;
using Monster.Middle.Persist.Instance;
using Monster.Middle.Processes.Acumatica.Persist;
using Monster.Middle.Processes.Shopify.Persist;
using Monster.Middle.Processes.Sync.Model.Analysis;
using Monster.Middle.Processes.Sync.Model.Inventory;
using Monster.Middle.Processes.Sync.Model.Orders;
using Monster.Middle.Processes.Sync.Persist;
using Push.Foundation.Utilities.Helpers;
using Push.Foundation.Utilities.Json;

namespace Monster.Middle.Processes.Sync.Services
{
    public class AnalysisDataService
    {
        private readonly ProcessPersistContext _persistContext;
        private readonly ShopifyUrlService _shopifyUrlService;
        private readonly AcumaticaUrlService _acumaticaUrlService;
        private readonly AcumaticaHttpContext _acumaticaHttpContext;
        private readonly SalesOrderClient _salesOrderClient;
        private readonly SettingsRepository _settingsRepository;

        public AnalysisDataService(
                ProcessPersistContext persistContext, 
                ShopifyUrlService shopifyUrlService, 
                AcumaticaUrlService acumaticaUrlService, 
                AcumaticaHttpContext acumaticaHttpContext, 
                SalesOrderClient salesOrderClient, 
                SettingsRepository settingsRepository)
        {
            _persistContext = persistContext;
            _shopifyUrlService = shopifyUrlService;
            _acumaticaUrlService = acumaticaUrlService;
            _acumaticaHttpContext = acumaticaHttpContext;
            _salesOrderClient = salesOrderClient;
            _settingsRepository = settingsRepository;
        }

        public List<OrderAnalyzerResultsRow> GetOrderAnalysisResults(AnalyzerRequest request)
        {
            var queryable = GetOrderAnalysisQueryable(request);   
            var results = queryable
                .OrderByDescending(x => x.ShopifyOrderId)
                .Skip(request.StartRecord)
                .Take(request.PageSize)
                .ToList();

            return results.Select(x => MakeOrderAnalyzerResults(x)).ToList();
        }

        public int GetOrderAnalysisRecordCount(AnalyzerRequest request)
        {
            return GetOrderAnalysisQueryable(request).Count();
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

        private IQueryable<ShopifyOrder> GetOrderAnalysisQueryable(AnalyzerRequest request)
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

        public OrderAnalysisTotals GetOrderFinancialSummary(
                long shopifyOrderId, bool includeAcumaticaTotals = false)
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

            if (shopifyOrderRecord.ExistsInAcumatica())
            {
                output.AcumaticaSalesOrderNbr = shopifyOrderRecord.AcumaticaSalesOrder.AcumaticaOrderNbr;
                output.AcumaticaSalesOrderHref
                    = _acumaticaUrlService.AcumaticaSalesOrderUrl(
                            SalesOrderType.SO, shopifyOrderRecord.AcumaticaSalesOrder.AcumaticaOrderNbr);

                if (includeAcumaticaTotals)
                {
                    InjectTotalsFromAcumatica(shopifyOrderRecord, output);
                }

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

        private OrderAnalyzerResultsRow MakeOrderAnalyzerResults(ShopifyOrder order)
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


        private IQueryable<ShopifyVariant> ProductStockItemQueryable =>
            _persistContext
                .Entities
                .ShopifyVariants
                .Where(x => x.IsMissing == false)
                .Include(x => x.ShopifyInventoryLevels)
                .Include(x => x.ShopifyProduct)
                .Include(x => x.AcumaticaStockItems)
                .Include(x => x.AcumaticaStockItems.Select(y => y.AcumaticaWarehouseDetails));

        private IQueryable<ShopifyVariant> GetProductStockItemQueryable(AnalyzerRequest request)
        {
            var queryable = ProductStockItemQueryable;

            foreach (var term in ParseSearchTerms(request.SearchText))
            {
                if (term.IsLong())
                {
                    var id = term.ToLong();
                    queryable = queryable.Where(
                        x => x.ShopifyProduct.ShopifyProductId == id || x.ShopifyVariantId == id);
                }
                else
                {
                    queryable = queryable.Where(
                        x => x.ShopifySku.Contains(term) ||
                             x.ShopifyTitle.Contains(term) ||
                             x.ShopifyProduct.ShopifyTitle.Contains(term) ||
                             x.AcumaticaStockItems.Any(y => y.ItemId.Contains(term)));
                }
            }

            return queryable;
        }

        public List<ProductStockItemResultsRow> GetProductStockItemResults(AnalyzerRequest request)
        {
            var queryable = GetProductStockItemQueryable(request);
            var results = queryable
                .OrderBy(x => x.ShopifySku)
                .Skip(request.StartRecord)
                .Take(request.PageSize)
                .ToList();

            var settings = _settingsRepository.RetrieveSettings();
            return results.Select(x => MakeProductStockItemResults(x, settings)).ToList();
        }

        public int GetProductStockItemCount(AnalyzerRequest request)
        {
            return GetProductStockItemQueryable(request).Count();
        }

        public ProductStockItemResultsRow 
                    MakeProductStockItemResults(ShopifyVariant variant, MonsterSetting settings)
        {
            var output = new ProductStockItemResultsRow();

            output.ShopifyProductId = variant.ShopifyProduct.ShopifyProductId;
            output.ShopifyProductTitle = variant.ShopifyProduct.ShopifyTitle;
            output.ShopifyProductUrl 
                = _shopifyUrlService.ShopifyProductUrl(variant.ShopifyProduct.ShopifyProductId);
            output.ShopifyVariantId = variant.ShopifyVariantId;
            output.ShopifyVariantTitle = variant.ShopifyTitle;
            output.ShopifyVariantSku = variant.ShopifySku;
            output.ShopifyVariantUrl 
                = _shopifyUrlService.ShopifyVariantUrl(
                        variant.ShopifyProduct.ShopifyProductId, variant.ShopifyVariantId);
            output.ShopifyVariantTax = variant.ShopifyIsTaxable ? "YES" : "NO";
            output.ShopifyVariantPrice = variant.ShopifyPrice;
            output.ShopifyVariantAvailQty 
                = variant.ShopifyInventoryLevels.Sum(x => x.ShopifyAvailableQuantity);

            output.IsShopifyProductDeleted = variant.ShopifyProduct.IsDeleted;
            output.IsShopifyVariantMissing = variant.IsMissing;

            if (variant.IsMatched())
            {
                var stockItemRecord = variant.MatchedStockItem();
                var stockItem = stockItemRecord.AcumaticaJson.DeserializeFromJson<StockItem>();

                output.AcumaticaItemId = stockItemRecord.ItemId;
                output.AcumaticaItemDesc = stockItemRecord.AcumaticaDescription;
                output.AcumaticaItemUrl = _acumaticaUrlService.AcumaticaStockItemUrl(stockItemRecord.ItemId);
                output.AcumaticaItemTax = stockItemRecord.IsTaxable(settings).YesNoNAPlainEnglish();
                output.AcumaticaItemPrice = (decimal)stockItem.DefaultPrice.value;
                output.AcumaticaItemAvailQty 
                    = stockItemRecord.AcumaticaWarehouseDetails.Sum(x => (int)x.AcumaticaQtyOnHand);
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
