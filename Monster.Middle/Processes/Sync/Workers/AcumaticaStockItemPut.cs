using System;
using System.Collections.Generic;
using System.Linq;
using Monster.Acumatica.Api;
using Monster.Acumatica.Api.Common;
using Monster.Acumatica.Api.Distribution;
using Monster.Middle.Misc.Logging;
using Monster.Middle.Persist.Instance;
using Monster.Middle.Processes.Acumatica.Persist;
using Monster.Middle.Processes.Shopify.Persist;
using Monster.Middle.Processes.Sync.Misc;
using Monster.Middle.Processes.Sync.Model.Inventory;
using Monster.Middle.Processes.Sync.Persist;
using Push.Foundation.Utilities.Json;
using Push.Foundation.Utilities.Logging;
using Push.Shopify.Api.Product;

namespace Monster.Middle.Processes.Sync.Workers
{
    public class AcumaticaStockItemPut
    {
        private readonly AcumaticaInventoryRepository _inventoryRepository;
        private readonly SyncInventoryRepository _syncRepository;
        private readonly DistributionClient _distributionClient;
        private readonly SettingsRepository _settingsRepository;
        private readonly ExecutionLogService _logService;

        public AcumaticaStockItemPut(
                AcumaticaInventoryRepository inventoryRepository,
                SyncInventoryRepository syncRepository,                    
                DistributionClient distributionClient,
                SettingsRepository settingsRepository,
                ExecutionLogService logService,
                IPushLogger logger)
        {
            _syncRepository = syncRepository;
            _inventoryRepository = inventoryRepository;
            _distributionClient = distributionClient;
            _settingsRepository = settingsRepository;
            _logService = logService;
        }

        public void RunImportToAcumatica(AcumaticaStockItemImportContext context)
        {
            foreach (var shopifyProductId in context.ShopifyProductIds)
            {
                var variants = _syncRepository.RetrieveUnmatchedVariants(shopifyProductId);

                foreach (var variant in variants)
                {
                    RunStockItemImport(context, variant);
                }

                if (context.CreateInventoryReceipts)
                {
                    RunInventoryReceiptImport(shopifyProductId, context.VariantsForInventoryReceipt);
                }
            }
        }

        private void RunStockItemImport(AcumaticaStockItemImportContext context, ShopifyVariant variant)
        {
            var matchingShopifySkus 
                = _syncRepository.RetrieveNonMissingVariants(variant.StandardizedSku());

            if (matchingShopifySkus.Count > 1)
            {
                _logService.Log($"Stock Item Import: {variant.LogDescriptor()} has duplicates in Shopify - aborting");
                return;
            }

            // Attempt to Auto-match
            //
            if (variant.IsMatched())
            {
                _logService.Log($"Stock Item Import: {variant.LogDescriptor()} already matched - aborting");
                return;
            }

            var stockItem = _syncRepository.RetrieveStockItem(variant.StandardizedSku());

            if (stockItem != null)
            {
                if (stockItem.IsSynced())
                {
                    var msg = $"Stock Item Import: {variant.LogDescriptor()} SKU already synchronized";
                    _logService.Log(msg);
                    return;
                }
                else
                { 
                    var msg = $"Stock Item Import: auto-matched {stockItem.LogDescriptor()} to {variant.LogDescriptor()}";
                    _logService.Log(msg);

                    _syncRepository.InsertItemSync(variant, stockItem, context.IsSyncEnabled);
                    return;
                }
            }

            // With neither duplicates or Auto-matching having succeeded,
            // ... we'll create a new Stock Item in Acumatica
            //
            StockItemPush(context, variant);
            context.VariantsForInventoryReceipt.Add(variant);
        }

        public void StockItemPush(AcumaticaStockItemImportContext context, ShopifyVariant variant)
        {
            _logService.Log(LogBuilder.CreateStockItem(variant));

            var newStockItem = BuildNewStockItem(variant);
            var newStockItemJson = newStockItem.SerializeToJson();

            // Push to Acumatica API
            //
            var result = _distributionClient.AddNewStockItem(newStockItemJson);
            var item = result.DeserializeFromJson<StockItem>();

            // Create Monster record
            //
            var newRecord = new AcumaticaStockItem();

            newRecord.ItemId = item.InventoryID.value;
            newRecord.AcumaticaJson = item.SerializeToJson();
            newRecord.AcumaticaDescription = item.Description.value;
            newRecord.AcumaticaTaxCategory = item.TaxCategory.value;
            newRecord.IsPriceSynced = false;
            newRecord.DateCreated = DateTime.UtcNow;
            newRecord.LastUpdated = DateTime.UtcNow;

            using (var transaction = _syncRepository.BeginTransaction())
            {
                _inventoryRepository.InsertStockItems(newRecord);
                _syncRepository.InsertItemSync(variant, newRecord, context.IsSyncEnabled);

                var log = $"Created Stock Item {item.InventoryID.value} in Acumatica";
                _logService.Log(log);
                transaction.Commit();
            }
        }

        public void RunInventoryReceiptImport(long shopifyProductId, List<ShopifyVariant> variants)
        {
            var inventoryForSyncing = variants.SelectMany(x => x.ShopifyInventoryLevels).ToList();

            // Push Inventory Receipt to Acumatica API
            //
            var receipt = BuildReceipt(inventoryForSyncing);
            if (receipt.ControlQty.value == 0)
            {
                // Empty Receipt
                return;
            }

            // Execution logging...
            //
            var shopifyProduct = _syncRepository.RetrieveProduct(shopifyProductId);
            var msg = $"Creating Inventory Receipt for {shopifyProduct.LogDescriptor()} ({variants.Count} variants)";
            _logService.Log(msg);

            // Create Inventory Receipt in Acumatica
            //
            var resultJson = _distributionClient.AddInventoryReceipt(receipt.SerializeToJson());
            var resultObject = resultJson.DeserializeFromJson<InventoryReceipt>();

            // Create Monster Record
            //
            var monsterReceipt = new AcumaticaInventoryReceipt();
            monsterReceipt.AcumaticaRefNumber = resultObject.ReferenceNbr.value;
            monsterReceipt.AcumaticaJson = resultJson;
            monsterReceipt.IsReleased = false;
            monsterReceipt.DateCreated = DateTime.UtcNow;
            monsterReceipt.LastUpdate = DateTime.UtcNow;

            // No transaction - keep writing until you can't write no mo'!
            //
            _inventoryRepository.InsertInventoryReceipt(monsterReceipt);

            foreach (var level in inventoryForSyncing)
            {
                _syncRepository.InsertInventoryReceiptSync(level, monsterReceipt);
            }
        }


        private StockItem BuildNewStockItem(ShopifyVariant variant)
        {
            var settings = _settingsRepository.RetrieveSettings();
            var defaultItemClass = settings.AcumaticaDefaultItemClass;
            var defaultPostingClass = settings.AcumaticaDefaultPostingClass;

            var defaultTaxCategory = variant.ShopifyIsTaxable.TaxCategory(settings);

            var warehouses = _inventoryRepository.RetrieveWarehouses();
            var defaultWarehouseId = warehouses.First().AcumaticaWarehouseId;

            var shopifyVariant = variant.ShopifyVariantJson.DeserializeFromJson<Variant>();
            var shopifyProduct = variant.ShopifyProduct.ShopifyJson.DeserializeFromJson<Product>();

            var newStockItem = new StockItem();
            newStockItem.InventoryID = variant.StandardizedSku().ToValue();
            newStockItem.Description = Canonizers.StandardizedStockItemTitle(shopifyProduct, shopifyVariant).ToValue();

            newStockItem.DefaultPrice = ((double) shopifyVariant.price).ToValue();
            newStockItem.DefaultWarehouseID = defaultWarehouseId.ToValue();

            newStockItem.ItemClass = defaultItemClass.ToValue();
            newStockItem.PostingClass = defaultPostingClass.ToValue();
            newStockItem.TaxCategory = defaultTaxCategory.ToValue();
            return newStockItem;
        }

        private InventoryReceipt BuildReceipt(List<ShopifyInventoryLevel> inventory)
        {
            var postingDate = DateTime.UtcNow.Date;

            var controlQty = inventory.ControlQty();
            var controlCost = inventory.CogsControlTotal();

            var receipt = new InventoryReceipt();
            receipt.Date = postingDate.ToValue();
            receipt.ControlCost = controlCost.ToValue();
            receipt.ControlQty = ((double)controlQty).ToValue();
            receipt.Details = new List<InventoryReceiptDetails>();
            receipt.Hold = false.ToValue();

            foreach (var inventoryLevel in inventory)
            {
                var variant = inventoryLevel.ShopifyVariant;
                var location = inventoryLevel.ShopifyLocation;
                var stockItemId = variant.AcumaticaStockItemId();

                var unitCogs = (double)inventoryLevel.ShopifyVariant.ShopifyCost;

                var qty = (double) inventoryLevel.ShopifyAvailableQuantity;
                var warehouseId = location.AcumaticaWarehouseId();
                
                var detail = new InventoryReceiptDetails();
                detail.InventoryID = stockItemId.ToValue();
                detail.UnitCost = unitCogs.ToValue();
                detail.Qty = qty.ToValue();
                detail.WarehouseID = warehouseId.ToValue();

                receipt.Details.Add(detail);
            }

            return receipt;
        }
    }
}
