using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using Monster.Middle.Persist.Multitenant;
using Monster.Middle.Persist.Multitenant.Etc;
using Monster.Middle.Persist.Multitenant.Shopify;
using Monster.Middle.Processes.Inventory.Workers;
using Push.Foundation.Utilities.Json;
using Push.Foundation.Utilities.Logging;
using Push.Shopify.Api;
using Push.Shopify.Api.Customer;
using Push.Shopify.Api.Order;


namespace Monster.Middle.Processes.Orders.Workers
{
    public class ShopifyOrderPull
    {
        private readonly OrderApi _orderApi;
        private readonly CustomerApi _customerApi;
        private readonly ShopifyOrderRepository _orderRepository;
        private readonly TenantRepository _tenantRepository;
        private readonly BatchStateRepository _batchStateRepository;
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
                    BatchStateRepository batchStateRepository,
                    TenantRepository tenantRepository,
                    ShopifyInventoryRepository inventoryRepository)
        {
            _logger = logger;
            _orderApi = orderApi;
            _customerApi = customerApi;
            _orderRepository = orderRepository;
            _batchStateRepository = batchStateRepository;
            _inventoryRepository = inventoryRepository;
            _tenantRepository = tenantRepository;
        }


        public void RunAll()
        {
            _logger.Debug("Baseline Pull Shopify Orders");

            var preferences = _tenantRepository.RetrievePreferences();

            var startOfPullRun = DateTime.UtcNow;

            var firstFilter = new SearchFilter();
            firstFilter.OrderByCreatedAt();
            firstFilter.CreatedAtMinUtc = preferences.DataPullStart;


            var firstJson = _orderApi.Retrieve(firstFilter);
            var firstOrders
                = firstJson.DeserializeToOrderList().orders;

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

            _batchStateRepository
                .UpdateShopifyOrdersPullEnd(orderBatchEnd);
        }

        public void RunUpdated()
        {
            var batchState = _batchStateRepository.RetrieveBatchState();

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

            _batchStateRepository.UpdateShopifyOrdersPullEnd(startOfPullRun);
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
                UpsertOrderAndCustomer(order);
                UpsertOrderLineItems(order);
                UpsertOrderFulfillments(order);
                UpsertOrderRefunds(order);

                AutoAssignVariantForLineItems(order);
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
                newOrder.ShopifyOrderNumber = order.order_number.ToString();
                newOrder.ShopifyIsCancelled = order.cancelled_at != null;
                newOrder.ShopifyJson = order.SerializeToJson();
                newOrder.ShopifyFinancialStatus = order.financial_status;
                newOrder.CustomerMonsterId = monsterCustomerRecord.Id;
                newOrder.DateCreated = DateTime.UtcNow;
                newOrder.LastUpdated = DateTime.UtcNow;

                using (var scope = new TransactionScope())
                {
                    _orderRepository.InsertOrder(newOrder);
                    UpsertOrderLineItems(order);

                    scope.Complete();
                }
            }
            else
            {
                existingOrder.ShopifyJson = order.SerializeToJson();
                existingOrder.ShopifyIsCancelled = order.cancelled_at != null;
                existingOrder.ShopifyFinancialStatus = order.financial_status;
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
                var customer = customerJson.DeserializeFromJson<Customer>();

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

        public void UpsertOrderLineItems(Order order)
        {
            var orderRecord 
                = _orderRepository.RetrieveOrder(order.id);

            foreach (var lineItem in order.line_items)
            {
                var shopifyLineItem
                    = orderRecord
                        .UsrShopifyOrderLineItems
                        .FirstOrDefault(x => x.ShopifyLineItemId == lineItem.id);

                if (shopifyLineItem == null)
                {
                    var orderLineItem = new UsrShopifyOrderLineItem();
                    orderLineItem.UsrShopifyOrder = orderRecord;
                    orderLineItem.UsrShopifyVariant = null;
                    orderLineItem.ShopifyLineItemId = lineItem.id;
                    orderLineItem.ShopifyProductId = lineItem.product_id;
                    orderLineItem.ShopifyVariantId = lineItem.variant_id;
                    orderLineItem.ShopifySku = lineItem.sku;

                    _orderRepository.InsertOrderLineItem(orderLineItem);
                }
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
                    newRecord.ShopifyFulfillmentId = fulfillment.id;
                    newRecord.ShopifyOrderId = order.id;
                    newRecord.ShopifyStatus = fulfillment.status;
                    newRecord.UsrShopifyOrder = orderRecord;
                    newRecord.DateCreated = DateTime.UtcNow;
                    newRecord.LastUpdated = DateTime.UtcNow;
                }
                else
                {
                    fulfillmentRecord.ShopifyStatus = fulfillment.status;
                    fulfillmentRecord.LastUpdated = DateTime.UtcNow;
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
                }
                else
                {
                    // TODO - do we need this?
                    refundRecord.LastUpdated = DateTime.UtcNow;
                }
            }
        }

        public void AutoAssignVariantForLineItems(Order order)
        {
            long shopifyOrderId = order.id;
            var orderRecord = _orderRepository.RetrieveOrder(shopifyOrderId);

            foreach (var lineItem in orderRecord.UsrShopifyOrderLineItems)
            {
                if (lineItem.UsrShopifyVariant != null)
                {
                    continue;
                }

                var monsterVariant =
                    _inventoryRepository
                        .RetrieveVariant(
                            lineItem.ShopifyVariantId, lineItem.ShopifySku);

                if (monsterVariant != null)
                {
                    lineItem.UsrShopifyVariant = monsterVariant;
                    _inventoryRepository.SaveChanges();
                }
            }
        }

    }
}

