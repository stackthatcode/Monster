using System;
using Monster.Middle.Persist.Instance;
using Monster.Middle.Processes.Acumatica.Persist;
using Monster.Middle.Processes.Sync.Model.Status;
using Monster.Middle.Processes.Sync.Persist;
using Monster.Middle.Processes.Sync.Persist.Matching;
using Push.Shopify.Api.Order;

namespace Monster.Middle.Processes.Sync.Status
{
    [Obsolete]
    public class ShipmentStatusService
    {
        private readonly SyncInventoryRepository _inventoryRepository;
        private readonly SyncOrderRepository _orderRepository;

        public ShipmentStatusService(
                SyncInventoryRepository inventoryRepository, 
                SyncOrderRepository orderRepository)
        {
            _inventoryRepository = inventoryRepository;
            _orderRepository = orderRepository;
        }


        public  ShipmentSyncReadiness
                    IsReadyToSyncWithAcumatica(ShopifyFulfillment fulfillmentRecord)
        {
            var output = new ShipmentSyncReadiness();

            var shopifyOrderRecord
                = _orderRepository.RetrieveShopifyOrder(fulfillmentRecord.ShopifyOrderId);

            var shopifyOrder = shopifyOrderRecord.ShopifyJson.DeserializeToOrder();

            var shopifyFulfillment
                = shopifyOrder.Fulfillment(fulfillmentRecord.ShopifyFulfillmentId);

            // Isolate the Warehouse
            var locationRecord =
                _inventoryRepository.RetrieveLocation(shopifyFulfillment.location_id);

            var warehouseId = locationRecord.MatchedWarehouse().AcumaticaWarehouseId;

            foreach (var line in shopifyFulfillment.line_items)
            {
                var variantRecord =
                    _inventoryRepository.RetrieveVariant(line.variant_id.Value, line.sku);

                var stockItemId = variantRecord.MatchedStockItem().ItemId;

                // Get Warehouse Details
                var stockItem = _inventoryRepository.RetrieveStockItem(stockItemId);
                var warehouseDetail = stockItem.WarehouseDetail(warehouseId);

                if (line.quantity > warehouseDetail.AcumaticaQtyOnHand)
                {
                    output.StockItemsWithInsuffientInventory.Add(line.sku);
                }
            }

            return output;
        }
    }
}
