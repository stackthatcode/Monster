using System;
using System.Collections.Generic;
using Monster.Middle.Persist.Multitenant;
using Push.Foundation.Utilities.Json;
using Push.Foundation.Utilities.Logging;
using Push.Shopify.Api;
using Push.Shopify.Api.Order;


namespace Monster.Middle.Processes.Orders.Workers
{
    public class ShopifyOrderPull
    {
        private readonly OrderApi _orderApi;
        private readonly OrderRepository _orderRepository;
        private readonly BatchStateRepository _batchStateRepository;
        private readonly IPushLogger _logger;

        // Possibly expand - this is a one-time thing...
        //
        public const int InitialBatchStateFudgeMin = -15;

        public ShopifyOrderPull(
                OrderApi orderApi, 
                OrderRepository orderRepository, 
                BatchStateRepository batchStateRepository, 
                IPushLogger logger)
        {
            _orderApi = orderApi;
            _orderRepository = orderRepository;
            _batchStateRepository = batchStateRepository;
            _logger = logger;
        }

        public void RunAll()
        {
            _logger.Debug("Baseline Pull Shopify Orders");

            _batchStateRepository.ResetBatchState();

            var startOfPullRun = DateTime.UtcNow;

            var firstFilter = new SearchFilter();
            firstFilter.Page = 1;

            var firstJson = _orderApi.Retrieve(firstFilter);
            var orders = firstJson.DeserializeFromJson<OrderList>().orders;

            UpsertOrders(orders);

            var currentPage = 2;

            while (true)
            {
                var currentFilter = firstFilter.Clone();
                currentFilter.Page = currentPage;

                var currentJson = _orderApi.Retrieve(currentFilter);
                var currentOrders = currentJson.DeserializeFromJson<OrderList>().orders;
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
            firstFilter.Page = 1;
            firstFilter.UpdatedAtMinUtc = lastBatchStateEnd;

            // Pull from Shopify
            var firstJson = _orderApi.Retrieve(firstFilter);
            var firstOrders = firstJson.DeserializeFromJson<OrderList>().orders;
            
            var currentPage = 2;

            while (true)
            {
                var currentFilter = firstFilter.Clone();
                currentFilter.Page = currentPage;

                // Pull from Shopify
                var currentJson = _orderApi.Retrieve(currentFilter);
                var currentOrders = currentJson.DeserializeFromJson<OrderList>().orders;

                if (currentOrders.Count == 0)
                {
                    break;
                }

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
            var existingOrder = _orderRepository.RetrieveShopifyOrder(order.id);

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
    }
}

