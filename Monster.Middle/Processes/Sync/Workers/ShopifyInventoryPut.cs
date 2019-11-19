using System;
using System.Linq;
using Monster.Acumatica.Api.Distribution;
using Monster.Middle.Misc.Logging;
using Monster.Middle.Persist.Instance;
using Monster.Middle.Processes.Shopify.Persist;
using Monster.Middle.Processes.Sync.Misc;
using Monster.Middle.Processes.Sync.Model.Inventory;
using Monster.Middle.Processes.Sync.Model.Status;
using Monster.Middle.Processes.Sync.Persist;
using Push.Foundation.Utilities.Json;
using Push.Foundation.Utilities.Logging;
using Push.Shopify.Api;
using Push.Shopify.Api.Inventory;
using Push.Shopify.Api.Product;


namespace Monster.Middle.Processes.Sync.Workers
{
    public class ShopifyInventoryPut
    {
        private readonly InventoryApi _inventoryApi;
        private readonly ProductApi _productApi;
        private readonly SyncInventoryRepository _syncInventoryRepository;
        private readonly ShopifyInventoryRepository _inventoryRepository;
        private readonly ExecutionLogService _executionLogService;
        private readonly IPushLogger _logger;

        public ShopifyInventoryPut(
                InventoryApi inventoryApi,
                ProductApi productApi,
                ShopifyInventoryRepository inventoryRepository, 
                SyncInventoryRepository syncInventoryRepository, 
                ExecutionLogService executionLogService, 
                IPushLogger logger)
        {
            _inventoryApi = inventoryApi;
            _productApi = productApi;
            _inventoryRepository = inventoryRepository;
            _syncInventoryRepository = syncInventoryRepository;
            _executionLogService = executionLogService;
            _logger = logger;
        }


        public void Run()
        {
            RunPriceUpdates();
            RunInventoryUpdate();
        }

        public void RunPriceUpdates()
        {
            var stockItems = _syncInventoryRepository.RetrieveStockItemsPriceNotSynced();

            foreach (var stockItem in stockItems)
            {
                var readyToSync = InventorySyncStatus.Make(stockItem).ReadyToSyncPrice();
                if (!readyToSync.Success)
                {
                    _logger.Debug($"Skipping PriceUpdate for {stockItem.ItemId}");
                    return;
                }

                _executionLogService.Log(LogBuilder.UpdateShopifyPrice(stockItem));
                PriceUpdate(stockItem);
            }
        }

        public void PriceUpdate(AcumaticaStockItem stockItem)
        {
            var stockItemRecord = _syncInventoryRepository.RetrieveStockItem(stockItem.ItemId);
            var stockItemObj = stockItemRecord.AcumaticaJson.DeserializeFromJson<StockItem>();

            if (!stockItemRecord.IsSynced())
            {
                return;
            }

            // Build the Shopify DTO
            //
            var variantShopifyId = stockItemRecord.MatchedVariant().ShopifyVariantId;
            var variantSku = stockItemRecord.MatchedVariant().ShopifySku;
            var price = stockItemObj.DefaultPrice.value;
            var dto = VariantPriceUpdateParent.Make(variantShopifyId, price);

            // Push the price update to Shopify API
            //
            _productApi.UpdateVariantPrice(variantShopifyId, dto.SerializeToJson());

            using (var transaction = _syncInventoryRepository.BeginTransaction())
            {
                var log = LogBuilder.UpdateShopifyVariantPrice(variantSku, (decimal)price);
                _executionLogService.Log(log);

                stockItem.IsPriceSynced = true;
                stockItem.LastUpdated = DateTime.UtcNow;
                _syncInventoryRepository.SaveChanges();

                transaction.Commit();
            }
        }

        public void RunInventoryUpdate()
        {
            var stockItems = _syncInventoryRepository.RetrieveStockItemInventoryNotSynced();

            foreach (var stockItem in stockItems)
            {
                var readyToSync = InventorySyncStatus.Make(stockItem).ReadyToSyncInventory();
                if (!readyToSync.Success)
                {
                    _logger.Debug($"Skipping StockItemInventoryUpdate for {stockItem.ItemId}");
                    return;
                }

                var content = LogBuilder.UpdateShopifyInventory(stockItem);
                _executionLogService.Log(content);
                InventoryUpdate(stockItem);
            }
        }

        public void InventoryUpdate(AcumaticaStockItem stockItem)
        {
            var variant
                = _inventoryRepository.RetrieveVariant(stockItem.MatchedVariant().ShopifyVariantId);

            if (variant.IsMissing)
            {
                _logger.Debug(
                    $"Skipping Inventory Update for " +
                    $"Variant {variant.ShopifySku} ({variant.ShopifyVariantId}) " +
                    $"StockItem {stockItem} - reason: Missing Variant");
                return;
            }

            if (variant.ShopifyProduct.IsDeleted)
            {
                _logger.Debug(
                    $"Skipping Inventory Update for " +
                    $"Variant {variant.ShopifySku} ({variant.ShopifyVariantId}) " +
                    $"StockItem {stockItem} - reason: Deleted Parent Product");
                return;
            }

            foreach (var level in variant.ShopifyInventoryLevels)
            {
                RunInventoryLevelUpdate(stockItem, level);
            }
        }

        public void RunInventoryLevelUpdate(AcumaticaStockItem stockItem, ShopifyInventoryLevel level)
        {
            var location = _syncInventoryRepository.RetrieveLocation(level.ShopifyLocationId);
            var warehouseIds = location.MatchedWarehouseIds();
            var details = stockItem.WarehouseDetails(warehouseIds);
                
            var totalQtyOnHand = (int)details.Sum(x => x.AcumaticaQtyOnHand);
            var sku = level.ShopifyVariant.ShopifySku;
            
            var levelDto = new InventoryLevel
            {
                inventory_item_id = level.ShopifyInventoryItemId,
                available = totalQtyOnHand,
                location_id = location.ShopifyLocationId,
            };

            var levelJson = levelDto.SerializeToJson();
            var result = _inventoryApi.SetInventoryLevels(levelJson);

            using (var transaction = _syncInventoryRepository.BeginTransaction())
            {
                var log = $"Updated Shopify Variant {sku} " +
                          $"in Location {location.ShopifyLocationName} to Available Qty {totalQtyOnHand}";

                _executionLogService.Log(log);

                // Flag Acumatica Warehouse Detail as synchronized
                //
                details.ForEach(x => x.IsInventorySynced = true);
                
                // Update Shopify Inventory Level records
                //
                level.ShopifyAvailableQuantity = totalQtyOnHand;
                level.LastUpdated = DateTime.UtcNow;

                _inventoryRepository.SaveChanges();
                transaction.Commit();
            }
        }
    }
}
