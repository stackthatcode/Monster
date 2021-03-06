﻿using System.Collections.Generic;
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
using Monster.Middle.Processes.Sync.Misc;
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
        private readonly SettingsRepository _settingsRepository;
        private readonly PendingActionService _pendingActionService;
        private readonly AcumaticaJsonService _acumaticaJsonService;
        private readonly AcumaticaHttpContext _acumaticaHttpContext;
        private readonly ShopifyJsonService _shopifyJsonService;
        private readonly SalesOrderClient _salesOrderClient;

        public AnalysisDataService(
                ProcessPersistContext persistContext, 
                ShopifyUrlService shopifyUrlService, 
                AcumaticaUrlService acumaticaUrlService, 
                SettingsRepository settingsRepository, 
                PendingActionService pendingActionService, 
                AcumaticaHttpContext acumaticaHttpContext, 
                SalesOrderClient salesOrderClient, 
                ShopifyJsonService shopifyJsonService, AcumaticaJsonService acumaticaJsonService)
        {
            _persistContext = persistContext;
            _shopifyUrlService = shopifyUrlService;
            _acumaticaUrlService = acumaticaUrlService;
            _settingsRepository = settingsRepository;
            _pendingActionService = pendingActionService;
            _acumaticaHttpContext = acumaticaHttpContext;
            _salesOrderClient = salesOrderClient;
            _shopifyJsonService = shopifyJsonService;
            _acumaticaJsonService = acumaticaJsonService;
        }

        public List<OrderAnalyzerResultsRow> GetOrderAnalysisResults(AnalyzerRequest request)
        {
            var queryable = GetOrderAnalysisQueryable(request);   
            var results = queryable
                .OrderByDescending(x => x.ShopifyOrderId)
                .Skip(request.StartRecord)
                .Take(request.PageSize)
                .ToList();

            var output = new List<OrderAnalyzerResultsRow>();
            foreach (var result in results)
            {
                output.Add(MakeOrderAnalyzerResults(result));
            }
            return output;
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
                .Include(x => x.AcumaticaSalesOrder.AcumaticaSoShipments.Select(y => y.ShopifyFulfillment))
                .Include(x => x.ShopifyTransactions)
                .Include(x => x.ShopifyTransactions.Select(y => y.AcumaticaPayment))
                .Include(x => x.ShopifyRefunds);

        private IQueryable<ShopifyOrder> GetOrderAnalysisQueryable(AnalyzerRequest request)
        {
            var queryable = ShopifyOrderQueryable;

            foreach (var term in ParseSearchTerms(request.SearchText))
            {
                if (term.IsLong())
                {
                    var longIdentifier = term.ToLong();
                    queryable = queryable.Where(
                        x => x.ShopifyOrderId == longIdentifier ||
                             x.ShopifyOrderNumber.Contains(term) ||
                             x.ShopifyTransactions.Any(y => y.ShopifyTransactionId == longIdentifier) ||
                             x.AcumaticaSalesOrder.AcumaticaOrderNbr.Contains(term));
                }
                else
                {
                    queryable = queryable.Where(
                        x => x.ShopifyOrderNumber.Contains(term) ||
                             x.AcumaticaSalesOrder.AcumaticaOrderNbr.Contains(term));
                }
            }

            if (request.OrderStatus == AnalyzerStatus.Errors)
            {
                queryable = queryable.Where(x => x.ErrorCount >= SystemConsts.ErrorThreshold);
            }

            if (request.OrderStatus == AnalyzerStatus.Unsynced)
            {
                queryable 
                    = queryable.Join(
                        _persistContext.Entities.ShopifyOrdersNeedingSyncAlls,
                        ord => ord.MonsterId,
                        vw => vw.MonsterId,
                        (ord, vw) => ord);
            }

            if (request.OrderStatus == AnalyzerStatus.Synced)
            {
                queryable
                    = queryable.Join(
                        _persistContext.Entities.ShopifyOrdersNotNeedingSyncAlls,
                        ord => ord.MonsterId,
                        vw => vw.MonsterId,
                        (ord, vw) => ord);
            }

            if (request.OrderStatus == AnalyzerStatus.Blocked)
            {
                queryable = queryable.Where(x => x.Ignore == true);
            }

            return queryable;
        }

        public OrderAnalysisTotals GetOrderFinancialSummary(long shopifyOrderId)
        {
            var shopifyOrderRecord = 
                ShopifyOrderQueryable.FirstOrDefault(x => x.ShopifyOrderId == shopifyOrderId);

            var shopifyOrder = _shopifyJsonService.RetrieveOrder(shopifyOrderId);
            
            var output = new OrderAnalysisTotals();
            output.ShopifyOrderNbr = shopifyOrderRecord.ShopifyOrderNumber;
            output.ShopifyOrderId = shopifyOrderRecord.ShopifyOrderId;
            output.ShopifyOrderHref = _shopifyUrlService.ShopifyOrderUrl(shopifyOrderRecord.ShopifyOrderId);

            output.ShopifyCustomerId = shopifyOrderRecord.ShopifyCustomer.ShopifyCustomerId;
            output.ShopifyCustomerHref =
                _shopifyUrlService
                    .ShopifyCustomerUrl(shopifyOrderRecord.ShopifyCustomer.ShopifyCustomerId);

            output.ShopifyTotalLinePrice = shopifyOrder.LineItemAmountAfterDiscountAndRefund;
            output.ShopifyShippingPriceTotal = shopifyOrder.NetShippingPrice;
            output.ShopifyTotalTax = shopifyOrder.NetTax;
            output.ShopifyOrderTotal = shopifyOrder.NetOrderTotal;

            output.ShopifyOrderPayment = shopifyOrderRecord.ShopifyPaymentAmount();
            output.ShopifyRefundPayment = shopifyOrderRecord.RefundTransactions().Sum(x => x.ShopifyAmount);
            output.ShopifyNetPayment = shopifyOrderRecord.ShopifyNetPayment();

            output.ShopifyRefundItemTotal = shopifyOrder.RefundLineItemTotal;
            output.ShopifyRefundShippingTotal = shopifyOrder.RefundShippingTotal;
            output.ShopifyRefundTaxTotal = shopifyOrder.RefundTotalTax;
            output.ShopifyCreditTotal = shopifyOrder.RefundCreditTotal;
            output.ShopifyDebitTotal = shopifyOrder.RefundDebitTotal;
            output.ShopifyRefundTotal = shopifyOrder.RefundTotal;
            output.ShopifyRefundOverpayment = shopifyOrder.RefundOverpayment;

            if (shopifyOrderRecord.ExistsInAcumatica() &&
                shopifyOrderRecord.AcumaticaSalesOrder.AcumaticaOrderNbr != AcumaticaSyncConstants.BlankRefNbr)
            {
                output.AcumaticaSalesOrderNbr 
                    = shopifyOrderRecord.AcumaticaSalesOrder.AcumaticaOrderNbr ?? "BLANK";
                output.AcumaticaSalesOrderHref
                    = _acumaticaUrlService.AcumaticaSalesOrderUrl(
                        SalesOrderType.SO, shopifyOrderRecord.AcumaticaSalesOrder.AcumaticaOrderNbr);

                output.AcumaticaCustomerNbr = shopifyOrderRecord.AcumaticaSalesOrder.AcumaticaCustomer.AcumaticaCustomerId;
                output.AcumaticaCustomerHref 
                    = _acumaticaUrlService.AcumaticaCustomerUrl(
                        shopifyOrderRecord.AcumaticaSalesOrder.AcumaticaCustomer.AcumaticaCustomerId);

                var acumaticaOrder = shopifyOrderRecord.AcumaticaSalesOrder;
                output.AcumaticaOrderLineTotal = acumaticaOrder.AcumaticaLineTotal;
                output.AcumaticaOrderFreight = acumaticaOrder.AcumaticaFreight;
                output.AcumaticaTaxTotal = acumaticaOrder.AcumaticaTaxTotal;
                output.AcumaticaOrderTotal = acumaticaOrder.AcumaticaOrderTotal;
            }

            output.AcumaticaPaymentTotal = shopifyOrderRecord.AcumaticaPaymentAmount();
            output.AcumaticaRefundPaymentTotal = shopifyOrderRecord.AcumaticaCustomerRefundTotal();
            output.AcumaticaNetPaymentTotal = shopifyOrderRecord.AcumaticaNetPaymentAmount();

            output.AcumaticaRefundCreditTotal = shopifyOrderRecord.AcumaticaCreditMemosTotal();
            output.AcumaticaRefundDebitTotal = shopifyOrderRecord.AcumaticaDebitMemosTotal();

            output.AcumaticaInvoiceTaxTotal = shopifyOrderRecord.AcumaticaInvoiceTaxTotal();
            output.AcumaticaInvoiceTotal = shopifyOrderRecord.AcumaticaInvoiceTotal();
            
            return output;
        }

        private OrderAnalyzerResultsRow MakeOrderAnalyzerResults(ShopifyOrder order)
        {
            var output = new OrderAnalyzerResultsRow();
            output.ShopifyOrderId = order.ShopifyOrderId;
            output.ShopifyOrderNbr = order.ShopifyOrderNumber.ToString();
            output.ShopifyOrderHref = _shopifyUrlService.ShopifyOrderUrl(order.ShopifyOrderId);

            // TODO - need to add a flag that indicates this Order has been archived
            //
            var shopifyOrder = _shopifyJsonService.RetrieveOrder(order.ShopifyOrderId);

            output.ShopifyOrderTotal = shopifyOrder.NetOrderTotal;
            output.ShopifyNetPayment = order.ShopifyNetPayment();

            output.ShopifyFinancialStatus = order.ShopifyFinancialStatus;
            output.ShopifyFulfillmentStatus = order.ShopifyFulfillmentStatus;
            output.ShopifyIsCancelled = order.ShopifyIsCancelled;
            output.ShopifyAreAllItemsRefunded = order.ShopifyAreAllItemsRefunded;

            var acumaticaSalesOrder = order.AcumaticaSalesOrder;
            if (acumaticaSalesOrder != null && acumaticaSalesOrder.AcumaticaOrderNbr != AcumaticaSyncConstants.BlankRefNbr)
            {
                output.AcumaticaSalesOrderNbr =acumaticaSalesOrder.AcumaticaOrderNbr;
                output.AcumaticaSalesOrderHref =
                    _acumaticaUrlService
                        .AcumaticaSalesOrderUrl(SalesOrderType.SO, acumaticaSalesOrder.AcumaticaOrderNbr);
                output.AcumaticaStatus = acumaticaSalesOrder.AcumaticaStatus;
                output.AcumaticaOrderTotal = acumaticaSalesOrder.AcumaticaOrderTotal;
            }

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

            output.Ignore = order.Ignore;
            output.HasError = order.ExceedsErrorLimit();
            output.HasPendingActions = _pendingActionService.Create(order, false).HasPendingActions;

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


        private IQueryable<ShopifyVariant> ProductStockItemQueryable =>
            _persistContext
                .Entities
                .ShopifyVariants
                .Where(x => x.IsMissing == false)
                .Include(x => x.ShopifyInventoryLevels)
                .Include(x => x.ShopifyProduct)
                .Include(x => x.AcumaticaStockItems)
                .Include(x => x.AcumaticaStockItems.Select(y => y.AcumaticaInventories));

        private IQueryable<ShopifyVariant> GetProductStockItemQueryable(AnalyzerRequest request)
        {
            var queryable = ProductStockItemQueryable;

            if (request.SyncFilter == "Synced Only")
            {
                queryable = queryable.Where(x => x.AcumaticaStockItems.Any());
            }
            if (request.SyncFilter == "Unsynced Only")
            {
                queryable = queryable.Where(x => !x.AcumaticaStockItems.Any());
            }

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
            output.HasDuplicateSkus = HasDuplicateSkus(variant.ShopifySku);

            if (variant.IsMatched())
            {
                var stockItemRecord = variant.MatchedStockItem();
                var stockItem = _acumaticaJsonService.RetrieveStockItem(stockItemRecord.ItemId);

                output.AcumaticaItemId = stockItemRecord.ItemId;
                output.AcumaticaItemDesc = stockItemRecord.AcumaticaDescription;
                output.AcumaticaItemUrl = _acumaticaUrlService.AcumaticaStockItemUrl(stockItemRecord.ItemId);
                output.AcumaticaItemTax = stockItemRecord.IsTaxable(settings).YesNoNaPlainEnglish();
                output.AcumaticaItemPrice = (decimal)stockItem.DefaultPrice.value;
                output.AcumaticaItemAvailQty  = stockItemRecord.AcumaticaInventories.Sum(x => (int)x.AcumaticaAvailQty);

                output.HasMismatchedSku = variant.AreSkuAndItemIdMismatched();
                output.HasMismatchedTaxes = variant.AreTaxesMismatched(settings);
            }

            return output;
        }

        public bool HasDuplicateSkus(string variantSku)
        {
            var standardSku = variantSku.StandardizedSku();
            return _persistContext.Entities
                       .ShopifyVariants
                       .Count(x => x.ShopifySku == standardSku && x.IsMissing == false) > 1;
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
