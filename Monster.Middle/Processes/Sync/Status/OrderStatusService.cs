using System.Collections.Generic;
using Monster.Middle.Persist.Instance;
using Monster.Middle.Processes.Shopify.Persist;
using Monster.Middle.Processes.Sync.Model.Orders;
using Monster.Middle.Processes.Sync.Model.Status;
using Monster.Middle.Processes.Sync.Persist;
using Push.Shopify.Api.Order;

namespace Monster.Middle.Processes.Sync.Status
{
    public class OrderStatusService
    {
        private readonly SyncInventoryRepository _syncInventoryRepository;
        private readonly SyncOrderRepository _syncOrderRepository;
        private readonly SettingsRepository _settingsRepository;

        public OrderStatusService(
                SyncInventoryRepository inventoryRepository, 
                SyncOrderRepository orderRepository, 
                SettingsRepository settingsRepository)
        {
            _syncInventoryRepository = inventoryRepository;
            _syncOrderRepository = orderRepository;
            _settingsRepository = settingsRepository;
        }

        public OrderSyncStatus ShopifyOrderStatus(long shopifyOrderId)
        {
            var output = new OrderSyncStatus();
            var orderRecord = _syncOrderRepository.RetrieveShopifyOrder(shopifyOrderId);
            var Settingss = _settingsRepository.RetrieveSettingss();

            output.ShopifyOrderId = shopifyOrderId;
            output.ShopifyOrderNumber = orderRecord.ShopifyOrderNumber;

            // If the Starting Shopify Order weren't populated, we would not be here i.e.
            // ... the Shopify Order would not have been pulled from API
            //
            output.SettingssStartingOrderId = Settingss.ShopifyOrderId.Value;
                
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

        private List<LineItem> LineItemsWithUnmatchedVariants(ShopifyOrder orderRecord)
        {
            var order = orderRecord.ToShopifyObj();
            var output = new List<LineItem>();

            foreach (var lineItem in order.line_items)
            {
                if (!lineItem.variant_id.HasValue || lineItem.sku == null)
                {
                    output.Add(lineItem);
                    continue;
                }

                var variant =
                    _syncInventoryRepository.RetrieveVariant(lineItem.variant_id.Value, lineItem.sku);

                if (variant == null || variant.IsNotMatched())
                {
                    output.Add(lineItem);
                    continue;
                }
            }

            return output;
        }

    }
}

