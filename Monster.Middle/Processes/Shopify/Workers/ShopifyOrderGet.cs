using System;
using System.Collections.Generic;
using System.Linq;
using Monster.Middle.Persist.Instance;
using Monster.Middle.Processes.Shopify.Persist;
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
        private readonly OrderApi _orderApi;


        public ShopifyOrderGet(
                ShopifyOrderRepository orderRepository,
                ShopifyBatchRepository batchRepository,
                SettingsRepository settingsRepository,
                ShopifyCustomerGet shopifyCustomerPull,
                ShopifyTransactionGet shopifyTransactionGet,
                OrderApi orderApi)
        {
            _orderRepository = orderRepository;
            _batchRepository = batchRepository;
            _settingsRepository = settingsRepository;
            _shopifyCustomerPull = shopifyCustomerPull;
            _shopifyTransactionGet = shopifyTransactionGet;
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
             
            var existingOrder = _orderRepository.RetrieveOrder(order.id);

            if (existingOrder == null)
            {
                var newOrder = new ShopifyOrder();

                newOrder.ShopifyOrderId = order.id;
                newOrder.ShopifyOrderNumber = order.name;
                newOrder.IsEmptyOrCancelled = order.IsEmptyOrCancelled;
                newOrder.ShopifyJson = order.SerializeToJson();
                newOrder.ShopifyFinancialStatus = order.financial_status;
                newOrder.NeedsTransactionGet = true;
                newOrder.NeedsOrderPut = true;
                newOrder.IsBlocked = false;
                newOrder.PutErrorCount = 0;
                newOrder.CustomerMonsterId = monsterCustomerRecord.Id;
                newOrder.DateCreated = DateTime.UtcNow;
                newOrder.LastUpdated = DateTime.UtcNow;

                _orderRepository.InsertOrder(newOrder);
            }
            else
            {
                existingOrder.ShopifyJson = order.SerializeToJson();

                if (existingOrder.IsEmptyOrCancelled != order.IsEmptyOrCancelled ||
                    existingOrder.ShopifyFinancialStatus != order.financial_status)
                {
                    existingOrder.NeedsOrderPut = true;
                }

                existingOrder.IsEmptyOrCancelled = order.IsEmptyOrCancelled;
                existingOrder.ShopifyFinancialStatus = order.financial_status;
                existingOrder.NeedsTransactionGet = true;
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
                        .ShopifyRefunds.FirstOrDefault(x => x.ShopifyRefundId == refund.id);

                if (refundRecord == null)
                {
                    orderRecord.NeedsOrderPut = true;

                    var newRecord = new ShopifyRefund();
                    newRecord.ShopifyRefundId = refund.id;
                    newRecord.ShopifyOrderId = order.id;
                    newRecord.CreditAdjustment = refund.CreditMemoTotal;
                    newRecord.DebitAdjustment = refund.DebitMemoTotal;
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

