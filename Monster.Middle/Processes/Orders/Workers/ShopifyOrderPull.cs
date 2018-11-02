using System;
using System.Collections.Generic;
using Monster.Middle.Persist.Multitenant;
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
        private readonly OrderRepository _orderRepository;
        private readonly TenantRepository _tenantRepository;
        private readonly BatchStateRepository _batchStateRepository;
        private readonly IPushLogger _logger;

        // Possibly expand - this is a one-time thing...
        //
        public const int InitialBatchStateFudgeMin = -15;

        public ShopifyOrderPull(
                    OrderApi orderApi, 
                    CustomerApi customerApi,
                    OrderRepository orderRepository, 
                    BatchStateRepository batchStateRepository, 
                    TenantRepository tenantRepository,
                    IPushLogger logger)
        {
            _orderApi = orderApi;
            _customerApi = customerApi;
            _orderRepository = orderRepository;
            _batchStateRepository = batchStateRepository;
            _logger = logger;
            _tenantRepository = tenantRepository;
        }


        public void RunAll()
        {
            _logger.Debug("Baseline Pull Shopify Orders");

            var preferences = _tenantRepository.RetrievePreferences();

            var startOfPullRun = DateTime.UtcNow;

            var firstFilter = new SearchFilter();
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
                _orderRepository.RetrieveShopifyOrderMaxUpdatedDate();

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
        
        private void UpsertOrders(List<Order> orders)
        {
            foreach (var order in orders)
            {
                UpsertOrder(order);
            }
        }

        private void UpsertOrder(Order order)
        {
            UpsertOrderCustomer(order);

            var existingOrder 
                = _orderRepository.RetrieveShopifyOrder(order.id);

            if (existingOrder == null)
            {
                var newOrder = new UsrShopifyOrder();
                newOrder.ShopifyOrderId = order.id;
                newOrder.ShopifyOrderNumber = order.order_number.ToString();
                newOrder.ShopifyIsCancelled = order.cancelled_at != null;
                newOrder.ShopifyJson = order.SerializeToJson();
                newOrder.DateCreated = DateTime.UtcNow;
                newOrder.LastUpdated = DateTime.UtcNow;
            }
            else
            {
                existingOrder.ShopifyJson = order.SerializeToJson();
                existingOrder.ShopifyIsCancelled = order.cancelled_at != null;
                existingOrder.LastUpdated = DateTime.UtcNow;
            }
        }

        private void UpsertOrderCustomer(Order order)
        {
            var existingCustomer =
                _orderRepository
                    .RetrieveShopifyCustomer(order.customer.id);

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
            }
            else
            {
                // Don't worry about updating customer record - it'll be
                // updated in that next run by ShopifyCustomerPull
            }
        }
    }
}

