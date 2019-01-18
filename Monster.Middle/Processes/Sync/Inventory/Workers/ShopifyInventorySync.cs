using System;
using System.Linq;
using Monster.Middle.Persist.Multitenant;
using Monster.Middle.Processes.Acumatica.Persist;
using Monster.Middle.Processes.Shopify.Persist;
using Monster.Middle.Processes.Sync.Extensions;
using Push.Foundation.Utilities.Json;
using Push.Foundation.Utilities.Logging;
using Push.Shopify.Api;
using Push.Shopify.Api.Inventory;

namespace Monster.Middle.Processes.Sync.Inventory.Workers
{
    public class ShopifyInventorySync
    {
        private readonly InventoryApi _inventoryApi;
        private readonly SyncInventoryRepository _syncInventoryRepository;
        private readonly ShopifyInventoryRepository _inventoryRepository;
        private readonly ExecutionLogRepository _executionLogRepository;
        private readonly IPushLogger _logger;

        public ShopifyInventorySync(
                InventoryApi inventoryApi,
                ShopifyInventoryRepository inventoryRepository, 
                SyncInventoryRepository syncInventoryRepository, 
                ExecutionLogRepository executionLogRepository, 
                IPushLogger logger)
        {
            _inventoryApi = inventoryApi;
            _inventoryRepository = inventoryRepository;
            _syncInventoryRepository = syncInventoryRepository;
            _executionLogRepository = executionLogRepository;
            _logger = logger;
        }


        public void Run()
        {
            var warehouseDetails
                    = _syncInventoryRepository.RetrieveWarehouseDetailsNotSynced();

            var warehouses = _syncInventoryRepository.RetrieveWarehouses();

            foreach (var warehouseDetail in warehouseDetails)
            {
                var quantity = (int)warehouseDetail.AcumaticaQtyOnHand;

                // Extracts the Shopify Location
                // NOTE - requires valid Warehouse-Location matching
                var warehouse = warehouses.ByDetail(warehouseDetail);                    
                var location = warehouse.MatchedLocation();

                if (location == null)
                {
                    _logger.Debug($"Acumatica Warehouse {warehouse.AcumaticaWarehouseId} not matched");
                    continue;
                }
                
                var locationMonsterId = location.MonsterId;
                var shopifyLocationId = location.ShopifyLocationId;

                // Extracts the Matched Variant
                var stockItem
                    = _syncInventoryRepository.RetrieveStockItem(
                        warehouseDetail.UsrAcumaticaStockItem.ItemId);

                if (stockItem.MatchedVariant() == null)
                {
                    _logger.Debug($"Acumatica Stock Item {stockItem.ItemId} not matched");
                    continue;
                }

                var inventoryItemId = stockItem.MatchedVariant().ShopifyInventoryItemId;
                var sku = stockItem.MatchedVariant().ShopifySku;
                
                var inventoryLevels =
                    stockItem.MatchedVariant().UsrShopifyInventoryLevels;

                var inventoryLevel =
                    inventoryLevels.First(
                        x => x.LocationMonsterId == locationMonsterId);

                var level = new InventoryLevel
                {
                    inventory_item_id = inventoryItemId,
                    available = quantity,
                    location_id = shopifyLocationId,
                };
                
                var levelJson = level.SerializeToJson();
                var result = _inventoryApi.SetInventoryLevels(levelJson);

                using (var transaction = _syncInventoryRepository.BeginTransaction())
                {
                    var log = $"Updated Shopify Variant {sku} " +
                              $"in Location {location.ShopifyLocationName} to {quantity}";
                    _executionLogRepository.InsertExecutionLog(log);

                    // Flag Acumatica Warehouse Detail as synchronized
                    warehouseDetail.IsShopifySynced = true;

                    // Update Shopify Inventory Level records
                    inventoryLevel.ShopifyAvailableQuantity = quantity;
                    inventoryLevel.LastUpdated = DateTime.UtcNow;
                    _inventoryRepository.SaveChanges();

                    transaction.Commit();
                }
            }
        }
    }
}
