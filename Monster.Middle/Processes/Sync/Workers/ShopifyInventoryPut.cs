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
        private readonly SettingsRepository _settingsRepository;
        private readonly ShopifyInventoryRepository _inventoryRepository;
        private readonly ExecutionLogService _executionLogService;
        private readonly IPushLogger _logger;

        public ShopifyInventoryPut(
                InventoryApi inventoryApi,
                ProductApi productApi,
                ShopifyInventoryRepository inventoryRepository, 
                SyncInventoryRepository syncInventoryRepository, 
                SettingsRepository settingsRepository,
                ExecutionLogService executionLogService, 
                IPushLogger logger)
        {
            _inventoryApi = inventoryApi;
            _productApi = productApi;
            _inventoryRepository = inventoryRepository;
            _syncInventoryRepository = syncInventoryRepository;
            _settingsRepository = settingsRepository;
            _executionLogService = executionLogService;
            _logger = logger;
        }


        public void Run()
        {
            return;
            //RunPriceUpdates();
            //RunInventoryUpdate();
        }

        private void RunPriceUpdates()
        {
            var stockItems = _syncInventoryRepository.RetrieveStockItemsPriceNotSynced();

            foreach (var stockItem in stockItems)
            {
                var readyToSync = MakeSyncStatus(stockItem).ReadyToSyncPrice();

                if (!readyToSync.Success)
                {
                    _logger.Debug($"Skipping PriceUpdate for {stockItem.ItemId}");
                    continue;
                }

                _executionLogService.Log(LogBuilder.UpdateShopifyPrice(stockItem));
                PriceAndCostUpdate(stockItem);
            }
        }

        public void PriceAndCostUpdate(AcumaticaStockItem stockItem, bool setTracking = false)
        {
            var settings = _settingsRepository.RetrieveSettings();
            var stockItemRecord = _syncInventoryRepository.RetrieveStockItem(stockItem.ItemId);
            var stockItemObj = stockItemRecord.AcumaticaJson.DeserializeFromJson<StockItem>();

            if (!stockItemRecord.IsMatched())
            {
                return;
            }

            var variantRecord = stockItemRecord.MatchedVariant();

            // Build the Shopify DTO
            //
            var variantShopifyId = variantRecord.ShopifyVariantId;
            var variantSku = variantRecord.ShopifySku;
            var price = (decimal)stockItemObj.DefaultPrice.value;
            var grams = stockItemObj.DimensionWeight.value.ToShopifyGrams();
            var taxable = stockItemRecord.IsTaxable(settings).Value;
            var costOfGoods = stockItem.AcumaticaLastCost;

            // Push the price update via Variant API
            //
            var priceDto = VariantPriceUpdateParent.Make(variantShopifyId, price, taxable, grams);
            _productApi.UpdateVariantPrice(variantShopifyId, priceDto.SerializeToJson());

            // Push the cost of goods via Inventory API
            //
            string content;

            if (setTracking)
            {
                var inventoryItem = new InventoryItem();
                inventoryItem.id = variantRecord.ShopifyInventoryItemId;
                inventoryItem.cost = costOfGoods;
                inventoryItem.tracked = true;
                content = new { inventory_item = inventoryItem }.SerializeToJson();
            }
            else
            {
                var inventoryItem = new InventoryItemUpdate();
                inventoryItem.id = variantRecord.ShopifyInventoryItemId;
                inventoryItem.cost = costOfGoods;
                content = new { inventory_item = inventoryItem }.SerializeToJson();
            }

            _inventoryApi.SetInventoryCost(variantRecord.ShopifyInventoryItemId, content);

            using (var transaction = _syncInventoryRepository.BeginTransaction())
            {
                var log = LogBuilder.UpdateShopifyVariantPrice(variantSku, price, taxable, costOfGoods, grams);
                _executionLogService.Log(log);

                stockItem.IsPriceSynced = true;
                stockItem.LastUpdated = DateTime.UtcNow;

                variantRecord.ShopifyPrice = price;
                variantRecord.ShopifyCost = costOfGoods;
                variantRecord.ShopifyIsTaxable = taxable;

                if (setTracking)
                {
                    variantRecord.ShopifyIsTracked = true;
                }

                _syncInventoryRepository.SaveChanges();
                transaction.Commit();
            }
        }

        public InventorySyncStatus MakeSyncStatus(AcumaticaStockItem input)
        {
            var settings = _settingsRepository.RetrieveSettings();

            var output = new InventorySyncStatus();
            output.StockItemId = input.ItemId;
            output.TaxCategoryId = input.AcumaticaTaxCategory;

            output.IsStockItemPriceSynced = input.IsPriceSynced;
            output.IsStockItemInventorySynced = input.AcumaticaInventories.All(x => x.IsInventorySynced);
            output.IsTaxCategoryValid = input.IsValidTaxCategory(settings);

            if (input.IsMatched())
            {
                var variant = input.MatchedVariant();
                output.ShopifyVariantId = variant.ShopifyVariantId;
                output.ShopifyVariantSku = variant.ShopifySku;
                output.IsShopifyVariantMissing = variant.IsMissing;
                output.ShopifyInventoryIsTracked = variant.ShopifyIsTracked;
            }
            else
            {
                output.IsShopifyVariantMissing = true;
            }

            return output;
        }

        private void RunInventoryUpdate()
        {
            var stockItems = _syncInventoryRepository.RetrieveStockItemInventoryNotSynced();

            foreach (var stockItem in stockItems)
            {
                var readyToSync = MakeSyncStatus(stockItem).ReadyToSyncInventory();
                if (!readyToSync.Success)
                {
                    _logger.Debug($"Skipping StockItemInventoryUpdate for {stockItem.ItemId}");
                    continue;
                }

                var content = LogBuilder.UpdateShopifyInventory(stockItem);
                _executionLogService.Log(content);
                InventoryUpdate(stockItem);
            }
        }

        public void InventoryUpdate(AcumaticaStockItem stockItem)
        {
            var variant = _inventoryRepository.RetrieveVariant(stockItem.MatchedVariant().ShopifyVariantId);

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
            var warehouseDetails = stockItem.Inventory(warehouseIds);
                
            var available = (int)warehouseDetails.Sum(x => x.AcumaticaAvailQty);
            var sku = level.ShopifyVariant.ShopifySku;
            
            var levelDto = new InventoryLevel
            {
                inventory_item_id = level.ShopifyInventoryItemId,
                available = available,
                location_id = location.ShopifyLocationId,
            };

            var levelJson = levelDto.SerializeToJson();
            _inventoryApi.SetInventoryLevels(levelJson);


            using (var transaction = _syncInventoryRepository.BeginTransaction())
            {
                var log = $"Updated Shopify Variant {sku} " +
                          $"in Location {location.ShopifyLocationName} to Available Qty {available}";

                _executionLogService.Log(log);

                warehouseDetails.ForEach(x => x.IsInventorySynced = true);
                
                // Update Shopify Inventory Level records
                //
                level.ShopifyAvailableQuantity = available;
                level.LastUpdated = DateTime.UtcNow;

                _inventoryRepository.SaveChanges();
                transaction.Commit();
            }
        }
    }
}
