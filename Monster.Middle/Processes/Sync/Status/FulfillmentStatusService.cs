using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Monster.Acumatica.Api.Shipment;
using Monster.Middle.Persist.Tenant;
using Monster.Middle.Processes.Shopify.Persist;
using Monster.Middle.Processes.Sync.Model.Extensions;
using Monster.Middle.Processes.Sync.Model.Status;
using Monster.Middle.Processes.Sync.Persist;
using Push.Foundation.Utilities.Json;

namespace Monster.Middle.Processes.Sync.Status
{
    public class FulfillmentStatusService
    {
        private readonly SyncInventoryRepository _syncInventoryRepository;
        private readonly SyncOrderRepository _syncOrderRepository;

        public FulfillmentStatusService(
                SyncInventoryRepository syncInventoryRepository, 
                SyncOrderRepository syncOrderRepository)
        {
            _syncInventoryRepository = syncInventoryRepository;
            _syncOrderRepository = syncOrderRepository;
        }


        public FulfillmentSyncReadiness
                    IsReadyToSyncWithShopify(UsrAcumaticaShipmentSalesOrderRef salesOrderRef)
        {
            var output = new FulfillmentSyncReadiness();
            var salesOrder = _syncOrderRepository.RetrieveSalesOrder(salesOrderRef.AcumaticaOrderNbr);
            var shopifyOrderId = salesOrder.MatchingShopifyOrder().ShopifyOrderId;

            // Fulfilled in Shopify - thus corrupted!
            output.AnyShopifyMadeFulfillments
                = _syncOrderRepository.AnyUnsyncedFulfillments(shopifyOrderId);

            // Unmatched Warehouse
            var shipmentRecord = salesOrderRef.UsrAcumaticaShipment;
            var shipment = shipmentRecord.AcumaticaJson.DeserializeFromJson<Shipment>();

            var warehouseRecord =
                _syncInventoryRepository.RetrieveWarehouse(shipment.WarehouseID.value);

            var locationRecord = warehouseRecord.MatchedLocation();
            if (locationRecord == null)
            {
                output.WarehouseLocationUnmatched = true;
            }

            // Unmatched Stock Item/Inventory
            foreach (var line in shipment.Details)
            {
                var stockItem = _syncInventoryRepository.RetrieveStockItem(line.InventoryID.value);
                var variant = stockItem.MatchedVariant();

                if (variant == null)
                {
                    output.UnmatchedVariantStockItems.Add(stockItem.ItemId);
                    continue;
                }

                var inventoryItem = variant.InventoryLevel(locationRecord.ShopifyLocationId);

                if (inventoryItem.ShopifyAvailableQuantity < line.ShippedQty.value)
                {
                    output.VariantsWithInsuffientInventory.Add(variant.ShopifySku);
                }
            }

            return output;
        }
    }
}

