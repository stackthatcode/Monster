using System;
using System.Collections.Generic;
using System.Linq;
using Monster.Middle.Misc.Hangfire;
using Monster.Middle.Misc.Logging;
using Monster.Middle.Persist.Instance;
using Monster.Middle.Processes.Shopify.Persist;
using Monster.Middle.Processes.Sync.Misc;
using Monster.Middle.Processes.Sync.Persist;
using Push.Foundation.Utilities.Json;
using Push.Shopify.Api;
using Push.Shopify.Api.Order;


namespace Monster.Middle.Processes.Shopify.Workers
{
    public class ShopifyOrderGet
    {
        private readonly ShopifyOrderRepository _orderRepository;
        private readonly ShopifyBatchRepository _batchRepository;
        private readonly SettingsRepository _settingsRepository;
        private readonly ShopifyCustomerGet _shopifyCustomerPull;
        private readonly ShopifyTransactionGet _shopifyTransactionGet;
        private readonly ExecutionLogService _executionLogService;
        private readonly JobMonitoringService _jobMonitoringService;
        private readonly ShopifyJsonService _shopifyJsonService;
        private readonly OrderApi _orderApi;



        public ShopifyOrderGet(
                ShopifyOrderRepository orderRepository,
                ShopifyBatchRepository batchRepository,
                SettingsRepository settingsRepository,
                ShopifyCustomerGet shopifyCustomerPull,
                ShopifyTransactionGet shopifyTransactionGet,
                JobMonitoringService jobMonitoringService,
                ExecutionLogService executionLogService, 
                ShopifyJsonService shopifyJsonService,
                OrderApi orderApi)
        {
            _orderRepository = orderRepository;
            _batchRepository = batchRepository;
            _settingsRepository = settingsRepository;
            _shopifyCustomerPull = shopifyCustomerPull;
            _shopifyTransactionGet = shopifyTransactionGet;
            _jobMonitoringService = jobMonitoringService;
            _executionLogService = executionLogService;
            _shopifyJsonService = shopifyJsonService;
            _orderApi = orderApi;
        }

        public void RunAutomatic()
        {
            var settings = _settingsRepository.RetrieveSettings();
            var batchState = _batchRepository.Retrieve();

            if (batchState.ShopifyOrdersGetEnd.HasValue)
            {
                var filter = new SearchFilter();
                filter.OrderByUpdatedAt();
                filter.SinceId = settings.ShopifyOrderId.Value;
                filter.UpdatedAtMinUtc = batchState.ShopifyOrdersGetEnd.Value;

                Run(filter);
            }
            else
            {
                var filter = new SearchFilter();
                filter.OrderByCreatedAt();
                filter.SinceId = settings.ShopifyOrderId.Value;

                Run(filter);
            }
        }

        private void Run(SearchFilter filter)
        {
            if (_jobMonitoringService.DetectCurrentJobInterrupt())
            {
                return;
            }

            var startOfRun = DateTime.UtcNow;
            var firstJson = _orderApi.Retrieve(filter);
            var firstOrders = firstJson.DeserializeToOrderList().orders;

            UpsertOrders(firstOrders);
            var currentPage = 2;

            while (true)
            {
                if (_jobMonitoringService.DetectCurrentJobInterrupt())
                {
                    return;
                }

                var currentFilter = filter.Clone();
                currentFilter.Page = currentPage;

                var currentJson = _orderApi.Retrieve(currentFilter);
                var currentOrders = currentJson.DeserializeToOrderList().orders;
                if (currentOrders.Count == 0)
                {
                    break;
                }

                UpsertOrders(currentOrders);
                currentPage++;
            }

            // Set the Batch State end marker to the time when this run started
            //
            var orderBatchEnd = (startOfRun).SubtractFudgeFactor();
            _batchRepository.UpdateOrdersGetEnd(orderBatchEnd);
        }

        public OrderParent Run(long shopifyOrderId)
        {
            var orderJson = _orderApi.Retrieve(shopifyOrderId);
            var order = orderJson.DeserializeToOrderParent();
            UpsertOrder(order.order);
            return order;
        }


        private void UpsertOrders(List<Order> orders)
        {
            foreach (var order in orders)
            {
                if (order.customer == null)
                {
                    continue;
                }
                else
                {
                    if (_jobMonitoringService.DetectCurrentJobInterrupt())
                    {
                        return;
                    }

                    UpsertOrder(order);
                }
            }
        }

        private void UpsertOrder(Order order)
        {
            UpsertOrderAndCustomer(order);
            UpsertOrderFulfillments(order);
            UpsertOrderRefunds(order);

            _shopifyTransactionGet.RunTransactionIfPullNeeded(order.id);
        }

        private void UpsertOrderAndCustomer(Order order)
        {
            var monsterCustomerRecord = _shopifyCustomerPull.UpsertCustomer(order.customer);

            using (var transaction = _orderRepository.BeginTransaction())
            {
                var existingOrder = _orderRepository.RetrieveOrder(order.id);
                if (existingOrder == null)
                {
                    var newOrder = new ShopifyOrder();

                    newOrder.ShopifyOrderId = order.id;
                    newOrder.ShopifyOrderNumber = order.name;
                    newOrder.ShopifyTotalPrice = order.total_price;
                    newOrder.ShopifyFinancialStatus = order.financial_status;
                    newOrder.ShopifyFulfillmentStatus = order.fulfillment_status;
                    newOrder.ShopifyIsCancelled = order.IsCancelled;
                    newOrder.ShopifyAreAllItemsRefunded = order.AreAllLineItemsRefunded;
                    newOrder.ShopifyTotalQuantity = order.NetOrderedQuantity;

                    newOrder.NeedsOrderPut = false;
                    newOrder.NeedsTransactionGet = true;
                    newOrder.ErrorCount = 0;
                    newOrder.Ignore = false;

                    newOrder.CustomerMonsterId = monsterCustomerRecord.MonsterId;
                    newOrder.DateCreated = DateTime.UtcNow;
                    newOrder.LastUpdated = DateTime.UtcNow;

                    _executionLogService.Log(LogBuilder.DetectedNewShopifyOrder(newOrder));
                    _orderRepository.InsertOrder(newOrder);
                }
                else
                {
                    existingOrder.ShopifyTotalPrice = order.total_price;
                    existingOrder.ShopifyFinancialStatus = order.financial_status;
                    existingOrder.ShopifyFulfillmentStatus = order.fulfillment_status;
                    existingOrder.ShopifyIsCancelled = order.IsCancelled;
                    existingOrder.ShopifyAreAllItemsRefunded = order.AreAllLineItemsRefunded;
                    existingOrder.ShopifyTotalQuantity = order.NetOrderedQuantity;

                    if (existingOrder.StatusChangeDetected(order))
                    {
                        existingOrder.NeedsOrderPut = true;
                    }

                    existingOrder.NeedsTransactionGet = true;
                    existingOrder.LastUpdated = DateTime.UtcNow;

                    _executionLogService.Log(LogBuilder.DetectedUpdateShopifyOrder(existingOrder));
                    _orderRepository.SaveChanges();
                }

                _shopifyJsonService.Upsert(ShopifyJsonType.Order, order.id, order.SerializeToJson());
                transaction.Commit();
            }
        }


        public void UpsertOrderFulfillments(Order order)
        {
            var orderRecord = _orderRepository.RetrieveOrder(order.id);

            foreach (var fulfillment in order.fulfillments)
            {
                var fulfillmentRecord
                    = orderRecord
                        .ShopifyFulfillments
                        .FirstOrDefault(x => x.ShopifyFulfillmentId == fulfillment.id);

                if (fulfillmentRecord == null)
                {
                    var newRecord = new ShopifyFulfillment();
                    newRecord.ShopifyOrderMonsterId = orderRecord.MonsterId;
                    newRecord.ShopifyFulfillmentId = fulfillment.id;
                    newRecord.ShopifyOrderId = order.id;
                    newRecord.ShopifyStatus = fulfillment.status;
                    newRecord.DateCreated = DateTime.UtcNow;
                    newRecord.LastUpdated = DateTime.UtcNow;

                    _orderRepository.InsertFulfillment(newRecord);
                }
                else
                {
                    fulfillmentRecord.ShopifyStatus = fulfillment.status;
                    fulfillmentRecord.LastUpdated = DateTime.UtcNow;

                    _orderRepository.SaveChanges();
                }
            }
        }

        private void UpsertOrderRefunds(Order order)
        {
            var orderRecord = _orderRepository.RetrieveOrder(order.id);

            foreach (var refund in order.refunds)
            {
                var refundRecord 
                    = orderRecord.ShopifyRefunds.FirstOrDefault(x => x.ShopifyRefundId == refund.id);

                if (refundRecord == null)
                {
                    orderRecord.NeedsOrderPut = true;

                    var newRecord = new ShopifyRefund();
                    newRecord.ShopifyRefundId = refund.id;
                    newRecord.ShopifyOrderId = order.id;
                    newRecord.ShopifyOrder = orderRecord;

                    newRecord.NeedOriginalPaymentPut = true;
                    newRecord.RequiresMemo =
                        Math.Abs(refund.CreditMemoTotal) > 0 || Math.Abs(refund.DebitMemoTotal) > 0;

                    newRecord.CreditAdjustment = refund.CreditMemoTotal;
                    newRecord.DebitAdjustment = refund.DebitMemoTotal;
                    newRecord.Shipping = refund.TotalShippingAdjustment;
                    newRecord.ShippingTax = refund.TotalShippingAdjustmentTax;

                    newRecord.DateCreated = DateTime.UtcNow;
                    newRecord.LastUpdated = DateTime.UtcNow;

                    _executionLogService.Log(LogBuilder.DetectedNewShopifyRefund(newRecord));
                    _orderRepository.InsertRefund(newRecord);
                }
                else
                {
                    refundRecord.LastUpdated = DateTime.UtcNow;
                    _orderRepository.SaveChanges();
                }
            }
        }
    }
}

