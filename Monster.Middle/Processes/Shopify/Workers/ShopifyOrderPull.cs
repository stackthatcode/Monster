using System;
using System.Collections.Generic;
using System.Linq;
using Monster.Middle.Persist.Tenant;
using Monster.Middle.Processes.Shopify.Persist;
using Monster.Middle.Processes.Sync.Persist;
using Monster.Middle.Processes.Sync.Services;
using Push.Foundation.Utilities.Json;
using Push.Foundation.Utilities.Logging;
using Push.Shopify.Api;
using Push.Shopify.Api.Order;


namespace Monster.Middle.Processes.Shopify.Workers
{
    public class ShopifyOrderPull
    {
        private readonly ShopifyOrderRepository _orderRepository;
        private readonly ShopifyBatchRepository _batchRepository;
        private readonly PreferencesRepository _preferencesRepository;
        private readonly ShopifyCustomerPull _shopifyCustomerPull;
        private readonly OrderApi _orderApi;
        private readonly IPushLogger _logger;


        public ShopifyOrderPull(
                ShopifyOrderRepository orderRepository,
                ShopifyBatchRepository batchRepository,
                PreferencesRepository preferencesRepository,
                ShopifyCustomerPull shopifyCustomerPull,
                OrderApi orderApi,
                IPushLogger logger)
        {
            _orderRepository = orderRepository;
            _batchRepository = batchRepository;
            _preferencesRepository = preferencesRepository;
            _shopifyCustomerPull = shopifyCustomerPull;
            _orderApi = orderApi;
            _logger = logger;
        }

        public void RunAutomatic()
        {
            var batchState = _batchRepository.Retrieve();
            if (batchState.ShopifyOrdersPullEnd.HasValue)
            {
                RunUpdated();
            }
            else
            {
                RunAll();
            }
        }

        private void RunAll()
        {
            _logger.Debug("ShopifyOrderPull -> RunAll()");

            var preferences = _preferencesRepository.RetrievePreferences();
            var startOfRun = DateTime.UtcNow;

            var firstFilter = new SearchFilter();
            firstFilter.OrderByCreatedAt();
            firstFilter.CreatedAtMinUtc = preferences.ShopifyOrderDateStart;
            
            var firstJson = _orderApi.Retrieve(firstFilter);
            var firstOrders = firstJson.DeserializeToOrderList().orders;

            UpsertOrders(firstOrders);

            var currentPage = 2;

            while (true)
            {
                var currentFilter = firstFilter.Clone();
                currentFilter.Page = currentPage;

                var currentJson = _orderApi.Retrieve(currentFilter);
                var currentOrders
                    = currentJson.DeserializeToOrderList().orders;

                UpsertOrders(currentOrders);

                if (currentOrders.Count == 0)
                {
                    break;
                }

                currentPage++;
            }

            // Compute the Batch State end marker
            var maxUpdatedDate = _orderRepository.RetrieveOrderMaxUpdatedDate();
            var orderBatchEnd = (maxUpdatedDate ?? startOfRun).AddShopifyBatchFudge();            

            _batchRepository.UpdateOrdersPullEnd(orderBatchEnd);
        }

        private void RunUpdated()
        {
            var startOfPullRun = DateTime.UtcNow;

            var batchState = _batchRepository.Retrieve();

            if (!batchState.ShopifyOrdersPullEnd.HasValue)
            {
                throw new Exception(
                    "ShopifyOrdersPullEnd not set - must run Baseline Pull first");
            }

            var lastBatchStateEnd = batchState.ShopifyOrdersPullEnd.Value;
            
            var firstFilter = new SearchFilter();
            firstFilter.OrderByUpdatedAt();
            firstFilter.UpdatedAtMinUtc = lastBatchStateEnd;

            // Pull from Shopify
            var firstJson = _orderApi.Retrieve(firstFilter);
            var firstOrders 
                    = firstJson.DeserializeFromJson<OrderList>().orders;
            UpsertOrders(firstOrders);

            var currentPage = 2;

            while (true)
            {
                var currentFilter = firstFilter.Clone();
                currentFilter.Page = currentPage;

                // Pull from Shopify
                var currentJson = _orderApi.Retrieve(currentFilter);
                var currentOrders
                    = currentJson.DeserializeFromJson<OrderList>().orders;

                if (currentOrders.Count == 0)
                {
                    break;
                }

                UpsertOrders(currentOrders);

                currentPage++;
            }

            _batchRepository.UpdateOrdersPullEnd(startOfPullRun);
        }

        public void Run(long shopifyOrderId)
        {
            var orderJson = _orderApi.Retrieve(shopifyOrderId);
            var order = orderJson.DeserializeToOrderParent();
            UpsertOrderAndCustomer(order.order);
        }


        private void UpsertOrders(List<Order> orders)
        {
            foreach (var order in orders)
            {
                if (order.customer == null)
                {
                    // TODO - add the Order Analysis service
                    continue;
                }

                UpsertOrderAndCustomer(order);
                UpsertOrderFulfillments(order);
                UpsertOrderRefunds(order);
            }
        }

        private void UpsertOrderAndCustomer(Order order)
        {
            var monsterCustomerRecord =
                _shopifyCustomerPull.UpsertCustomer(order.customer);
             
            var existingOrder = _orderRepository.RetrieveOrder(order.id);

            if (existingOrder == null)
            {
                var newOrder = new UsrShopifyOrder();
                newOrder.ShopifyOrderId = order.id;
                newOrder.ShopifyOrderNumber = order.order_number;
                newOrder.ShopifyIsCancelled = order.cancelled_at != null;
                newOrder.ShopifyJson = order.SerializeToJson();
                newOrder.ShopifyFinancialStatus = order.financial_status;
                newOrder.AreTransactionsUpdated = false;
                newOrder.CustomerMonsterId = monsterCustomerRecord.Id;
                newOrder.DateCreated = DateTime.UtcNow;
                newOrder.LastUpdated = DateTime.UtcNow;

                _orderRepository.InsertOrder(newOrder);
            }
            else
            {
                existingOrder.ShopifyJson = order.SerializeToJson();
                existingOrder.ShopifyIsCancelled = order.cancelled_at != null;
                existingOrder.ShopifyFinancialStatus = order.financial_status;
                existingOrder.AreTransactionsUpdated = false;
                existingOrder.LastUpdated = DateTime.UtcNow;

                _orderRepository.SaveChanges();
            }
        }
        

        public void UpsertOrderFulfillments(Order order)
        {
            var orderRecord = _orderRepository.RetrieveOrder(order.id);

            foreach (var fulfillment in order.fulfillments)
            {
                var fulfillmentRecord
                    = orderRecord
                        .UsrShopifyFulfillments
                        .FirstOrDefault(x => x.ShopifyFulfillmentId == fulfillment.id);

                if (fulfillmentRecord == null)
                {
                    var newRecord = new UsrShopifyFulfillment();
                    newRecord.OrderMonsterId = orderRecord.Id;
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
                    = orderRecord
                        .UsrShopifyRefunds
                        .FirstOrDefault(x => x.ShopifyRefundId == refund.id);

                if (refundRecord == null)
                {
                    var newRecord = new UsrShopifyRefund();
                    newRecord.ShopifyRefundId = refund.id;
                    newRecord.ShopifyOrderId = order.id;
                    newRecord.ShopifyIsCancellation = !order.fulfillments.Any();
                    newRecord.UsrShopifyOrder = orderRecord;
                    newRecord.DateCreated = DateTime.UtcNow;
                    newRecord.LastUpdated = DateTime.UtcNow;

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

