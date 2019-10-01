using System.Collections.Generic;
using Monster.Middle.Persist.Instance;
using Monster.Middle.Processes.Shopify.Persist;
using Monster.Middle.Processes.Sync.Model.Orders;
using Monster.Middle.Processes.Sync.Model.Status;
using Monster.Middle.Processes.Sync.Persist;
using Monster.Middle.Processes.Sync.Persist.Matching;
using Push.Shopify.Api.Order;

namespace Monster.Middle.Processes.Sync.Status
{
    public class OrderStatusService
    {
        private readonly SyncInventoryRepository _syncInventoryRepository;
        private readonly SyncOrderRepository _syncOrderRepository;
        private readonly PreferencesRepository _preferencesRepository;

        public OrderStatusService(
                SyncInventoryRepository inventoryRepository, 
                SyncOrderRepository orderRepository, 
                PreferencesRepository preferencesRepository)
        {
            _syncInventoryRepository = inventoryRepository;
            _syncOrderRepository = orderRepository;
            _preferencesRepository = preferencesRepository;
        }

        public OrderSyncStatus ShopifyOrderStatus(long shopifyOrderId)
        {
            var output = new OrderSyncStatus();
            var orderRecord = _syncOrderRepository.RetrieveShopifyOrder(shopifyOrderId);
            var preferences = _preferencesRepository.RetrievePreferences();

            output.ShopifyOrderId = shopifyOrderId;
            output.ShopifyOrderNumber = orderRecord.ShopifyOrderNumber;
            output.PreferencesStartingOrderNum = preferences.ShopifyOrderNumberStart;
                
            output.LineItemsWithAdhocVariants 
                        = orderRecord.ToShopifyObj().LineItemsWithAdhocVariants;
            output.LineItemsWithUnmatchedVariants = LineItemsWithUnmatchedVariants(orderRecord);

            output.IsPaid = orderRecord.IsPaid();
            output.IsCancelled = orderRecord.ShopifyIsCancelled;
            output.FulfillmentStatus = orderRecord.ToShopifyObj().fulfillment_status;

            output.AcumaticaSalesOrderId
                = orderRecord.HasMatch()
                    ? orderRecord.MatchingSalesOrder().AcumaticaOrderNbr : null;

            return output;
        }

        private List<LineItem> LineItemsWithUnmatchedVariants(UsrShopifyOrder orderRecord)
        {
            var order = orderRecord.ToShopifyObj();
            var output = new List<LineItem>();

            foreach (var lineItem in order.line_items)
            {
                var variant =
                    _syncInventoryRepository.RetrieveVariant(lineItem.variant_id.Value, lineItem.sku);

                if (variant == null || variant.IsNotMatched())
                {
                    output.Add(lineItem);
                }
            }

            return output;
        }



        //private OrderSyncSummary BuildOrderSummary()
        //{
        //    var output = new OrderSyncSummary();
        //    output.TotalOrders = _syncOrderRepository.RetrieveTotalOrders();
        //    output.TotalOrdersWithSalesOrders = _syncOrderRepository.RetrieveTotalOrdersSynced();
        //    output.TotalOrdersWithShipments = _syncOrderRepository.RetrieveTotalOrdersOnShipments();
        //    output.TotalOrdersWithInvoices = _syncOrderRepository.RetrieveTotalOrdersInvoiced();
        //    return output;
        //}

    }
}

