using System;
using System.Linq;
using Monster.Acumatica.Api.Distribution;
using Monster.Middle.Persist.Multitenant;
using Monster.Middle.Processes.Acumatica.Persist;
using Monster.Middle.Processes.Shopify.Persist;
using Monster.Middle.Processes.Sync.Extensions;
using Push.Foundation.Utilities.Json;
using Push.Foundation.Utilities.Logging;
using Push.Shopify.Api;
using Push.Shopify.Api.Inventory;
using Push.Shopify.Api.Product;

namespace Monster.Middle.Processes.Sync.Inventory.Workers
{
    public class ShopifyInventorySync
    {
        private readonly InventoryApi _inventoryApi;
        private readonly ProductApi _productApi;
        private readonly SyncInventoryRepository _syncInventoryRepository;
        private readonly ShopifyInventoryRepository _inventoryRepository;
        private readonly ExecutionLogRepository _executionLogRepository;
        private readonly IPushLogger _logger;

        public ShopifyInventorySync(
                InventoryApi inventoryApi,
                ProductApi productApi,
                ShopifyInventoryRepository inventoryRepository, 
                SyncInventoryRepository syncInventoryRepository, 
                ExecutionLogRepository executionLogRepository, 
                IPushLogger logger)
        {
            _inventoryApi = inventoryApi;
            _productApi = productApi;
            _inventoryRepository = inventoryRepository;
            _syncInventoryRepository = syncInventoryRepository;
            _executionLogRepository = executionLogRepository;
            _logger = logger;
        }


        public void Run()
        {
            RunPriceUpdates();
            RunInventoryLevels();
        }

        public void RunPriceUpdates()
        {
            var stockItems = _syncInventoryRepository.RetrieveStockItemsNotSynced();

            foreach (var stockItem in stockItems)
            {
                var stockItemRecord 
                    = _syncInventoryRepository.RetrieveStockItem(stockItem.ItemId);

                var stockItemObj
                    = stockItemRecord.AcumaticaJson.DeserializeFromJson<StockItem>();

                var variantShopifyId = stockItemRecord.MatchedVariant().ShopifyVariantId;
                var variantSku = stockItemRecord.MatchedVariant().ShopifySku;
                var price = stockItemObj.DefaultPrice.value;

                var dto = VariantPriceUpdateParent.Make(variantShopifyId, price);

                _productApi.UpdateVariantPrice(variantShopifyId, dto.SerializeToJson());

                using (var transaction = _syncInventoryRepository.BeginTransaction())
                {
                    var log = $"Updated Shopify Variant {variantSku} price to {price}";
                    _executionLogRepository.InsertExecutionLog(log);

                    stockItem.IsPriceSynced = true;
                    stockItem.LastUpdated = DateTime.UtcNow;
                    _syncInventoryRepository.SaveChanges();

                    transaction.Commit();
                }
            }
        }

        public void RunInventoryLevels()
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

                var levels = stockItem.MatchedVariant().UsrShopifyInventoryLevels;

                var level = levels.FirstOrDefault(x => x.LocationMonsterId == locationMonsterId);

                if (level == null)
                {
                    continue;
                }

                var levelDto = new InventoryLevel
                {
                    inventory_item_id = inventoryItemId,
                    available = quantity,
                    location_id = shopifyLocationId,
                };

                var levelJson = levelDto.SerializeToJson();
                var result = _inventoryApi.SetInventoryLevels(levelJson);

                using (var transaction = _syncInventoryRepository.BeginTransaction())
                {
                    var log = $"Updated Shopify Variant {sku} " +
                              $"in Location {location.ShopifyLocationName} to {quantity}";
                    _executionLogRepository.InsertExecutionLog(log);

                    // Flag Acumatica Warehouse Detail as synchronized
                    warehouseDetail.IsInventorySynced = true;

                    // Update Shopify Inventory Level records
                    level.ShopifyAvailableQuantity = quantity;
                    level.LastUpdated = DateTime.UtcNow;
                    _inventoryRepository.SaveChanges();

                    transaction.Commit();
                }
            }

        }
    }
}
