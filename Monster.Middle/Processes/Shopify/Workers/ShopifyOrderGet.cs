using System;
using System.Collections.Generic;
using System.Linq;
using Monster.Middle.Persist.Instance;
using Monster.Middle.Processes.Shopify.Persist;
using Monster.Middle.Processes.Sync.Model.Status;
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
        private readonly PreferencesRepository _preferencesRepository;
        private readonly ShopifyCustomerGet _shopifyCustomerPull;
        private readonly OrderApi _orderApi;


        public ShopifyOrderGet(
                ShopifyOrderRepository orderRepository,
                ShopifyBatchRepository batchRepository,
                PreferencesRepository preferencesRepository,
                ShopifyCustomerGet shopifyCustomerPull,
                OrderApi orderApi)
        {
            _orderRepository = orderRepository;
            _batchRepository = batchRepository;
            _preferencesRepository = preferencesRepository;
            _shopifyCustomerPull = shopifyCustomerPull;
            _orderApi = orderApi;
        }

        public void RunAutomatic()
        {
            var preferences = _preferencesRepository.RetrievePreferences();
            preferences.AssertStartingOrderIsValid();

            var batchState = _batchRepository.Retrieve();

            if (batchState.ShopifyOrdersGetEnd.HasValue)
            {
                var filter = new SearchFilter();
                filter.OrderByUpdatedAt();
                filter.SinceId = preferences.ShopifyOrderId.Value;
                filter.UpdatedAtMinUtc = batchState.ShopifyOrdersGetEnd.Value;

                Run(filter);
            }
            else
            {
                var filter = new SearchFilter();
                filter.OrderByCreatedAt();
                filter.SinceId = preferences.ShopifyOrderId.Value;

                Run(filter);
            }
        }

        private void Run(SearchFilter filter)
        {
            var startOfRun = DateTime.UtcNow;
            var firstJson = _orderApi.Retrieve(filter);
            var firstOrders = firstJson.DeserializeToOrderList().orders;

            UpsertOrders(firstOrders);
            var currentPage = 2;

            while (true)
            {
                var currentFilter = filter.Clone();
                currentFilter.Page = currentPage;

                var currentJson = _orderApi.Retrieve(currentFilter);
                var currentOrders = currentJson.DeserializeToOrderList().orders;

                UpsertOrders(currentOrders);

                if (currentOrders.Count == 0)
                {
                    break;
                }

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
            UpsertOrderAndCustomer(order.order);
            return order;
        }


        private void UpsertOrders(List<Order> orders)
        {
            foreach (var order in orders)
            {
                if (order.customer == null)
                {
                    // TODO - add the Order Analysis service
                    //
                    continue;
                }

                UpsertOrderAndCustomer(order);
                UpsertOrderFulfillments(order);
                UpsertOrderRefunds(order);
            }
        }

        private void UpsertOrderAndCustomer(Order order)
        {
            var monsterCustomerRecord = _shopifyCustomerPull.UpsertCustomer(order.customer);
             
            var existingOrder = _orderRepository.RetrieveOrder(order.id);

            if (existingOrder == null)
            {
                var newOrder = new ShopifyOrder();

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
                        .ShopifyFulfillments
                        .FirstOrDefault(x => x.ShopifyFulfillmentId == fulfillment.id);

                if (fulfillmentRecord == null)
                {
                    var newRecord = new ShopifyFulfillment();
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
                        .ShopifyRefunds
                        .FirstOrDefault(x => x.ShopifyRefundId == refund.id);

                if (refundRecord == null)
                {
                    var newRecord = new ShopifyRefund();
                    newRecord.ShopifyRefundId = refund.id;
                    newRecord.ShopifyOrderId = order.id;
                    newRecord.ShopifyIsCancellation = !order.fulfillments.Any();
                    newRecord.ShopifyOrder = orderRecord;
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

