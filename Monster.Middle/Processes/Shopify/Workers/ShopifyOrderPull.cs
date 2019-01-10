using System;
using System.Collections.Generic;
using System.Linq;
using Monster.Middle.Persist.Multitenant;
using Monster.Middle.Processes.Shopify.Persist;
using Push.Foundation.Utilities.Json;
using Push.Foundation.Utilities.Logging;
using Push.Shopify.Api;
using Push.Shopify.Api.Customer;
using Push.Shopify.Api.Order;

namespace Monster.Middle.Processes.Shopify.Workers
{
    public class ShopifyOrderPull
    {
        private readonly OrderApi _orderApi;
        private readonly CustomerApi _customerApi;
        private readonly ShopifyOrderRepository _orderRepository;
        private readonly ConnectionRepository _tenantRepository;
        private readonly ShopifyBatchRepository _shopifyBatchRepository;
        private readonly ShopifyInventoryRepository _inventoryRepository;
        private readonly IPushLogger _logger;

        // Possibly expand - this is a one-time thing...
        //
        public const int InitialBatchStateFudgeMin = -15;

        public ShopifyOrderPull(
                    IPushLogger logger,
                    OrderApi orderApi,
                    CustomerApi customerApi,
                    ShopifyOrderRepository orderRepository,
                    ShopifyBatchRepository shopifyBatchRepository,
                    ConnectionRepository tenantRepository,
                    ShopifyInventoryRepository inventoryRepository)
        {
            _logger = logger;
            _orderApi = orderApi;
            _customerApi = customerApi;
            _orderRepository = orderRepository;
            _shopifyBatchRepository = shopifyBatchRepository;
            _inventoryRepository = inventoryRepository;
            _tenantRepository = tenantRepository;
        }

        public void RunAutomatic()
        {
            var batchState = _shopifyBatchRepository.Retrieve();
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

            var preferences = _tenantRepository.RetrievePreferences();

            var startOfPullRun = DateTime.UtcNow;

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
            var maxUpdatedDate =
                _orderRepository.RetrieveOrderMaxUpdatedDate();

            var orderBatchEnd
                = maxUpdatedDate
                  ?? DateTime.UtcNow.AddMinutes(InitialBatchStateFudgeMin);

            _shopifyBatchRepository
                .UpdateShopifyOrdersPullEnd(orderBatchEnd);
        }

        private void RunUpdated()
        {
            var batchState = _shopifyBatchRepository.Retrieve();

            if (!batchState.ShopifyOrdersPullEnd.HasValue)
            {
                throw new Exception(
                    "ShopifyOrdersPullEnd not set - must run Baseline Pull first");
            }

            var lastBatchStateEnd = batchState.ShopifyOrdersPullEnd.Value;
            var startOfPullRun = DateTime.UtcNow; // Trick - we won't use this in filtering

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

            _shopifyBatchRepository.UpdateShopifyOrdersPullEnd(startOfPullRun);
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
            var monsterCustomerRecord = UpsertOrderCustomer(order);

            var existingOrder
                = _orderRepository.RetrieveOrder(order.id);

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

        private UsrShopifyCustomer UpsertOrderCustomer(Order order)
        {
            var existingCustomer =
                _orderRepository
                    .RetrieveCustomer(order.customer.id);

            if (existingCustomer == null)
            {
                var customerJson = _customerApi.Retrieve(order.customer.id);
                var customer
                    = customerJson.DeserializeFromJson<CustomerParent>()
                        .customer;

                var newCustomer = new UsrShopifyCustomer();
                newCustomer.ShopifyCustomerId = customer.id;
                newCustomer.ShopifyJson = customer.SerializeToJson();
                newCustomer.ShopifyPrimaryEmail = customer.email;
                newCustomer.DateCreated = DateTime.UtcNow;
                newCustomer.LastUpdated = DateTime.UtcNow;
                _orderRepository.InsertCustomer(newCustomer);

                return newCustomer;
            }
            else
            {
                return existingCustomer;
                // Don't worry about updating customer record - it'll be
                // updated in that next run by ShopifyCustomerPull
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

