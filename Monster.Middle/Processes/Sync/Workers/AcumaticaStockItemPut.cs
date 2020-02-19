using System;
using System.Collections.Generic;
using System.Linq;
using Monster.Acumatica.Api;
using Monster.Acumatica.Api.Common;
using Monster.Acumatica.Api.Distribution;
using Monster.Middle.Misc.Hangfire;
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
        private readonly ShopifyJsonService _shopifyJsonService;
        private readonly AcumaticaJsonService _acumaticaJsonService;
        private readonly JobMonitoringService _jobMonitoringService;

        public AcumaticaStockItemPut(
                AcumaticaInventoryRepository inventoryRepository,
                SyncInventoryRepository syncRepository,                    
                DistributionClient distributionClient,
                SettingsRepository settingsRepository,
                ExecutionLogService logService,
                ShopifyJsonService shopifyJsonService,
                JobMonitoringService jobMonitoringService, AcumaticaJsonService acumaticaJsonService)
        {
            _syncRepository = syncRepository;
            _inventoryRepository = inventoryRepository;
            _distributionClient = distributionClient;
            _settingsRepository = settingsRepository;
            _logService = logService;
            _shopifyJsonService = shopifyJsonService;
            _jobMonitoringService = jobMonitoringService;
            _acumaticaJsonService = acumaticaJsonService;
        }

        public void RunImportToAcumatica(AcumaticaStockItemImportContext context)
        {
            foreach (var shopifyProductId in context.ShopifyProductIds)
            {
                if (_jobMonitoringService.DetectCurrentJobInterrupt())
                {
                    return;
                }

                var variants = _syncRepository.RetrieveUnmatchedVariants(shopifyProductId);

                var variantsForReceipt = new List<ShopifyVariant>();

                foreach (var variant in variants)
                {
                    var result = RunStockItemImport(context, variant);

                    if (result == StockItemPutResult.CreatedStockItem && context.CreateInventoryReceipts)
                    {
                        variantsForReceipt.Add(variant);
                    }
                }

                if (context.CreateInventoryReceipts)
                {
                    RunInventoryReceiptImport(
                        context.WarehouseId, shopifyProductId, variantsForReceipt);
                }
            }
        }

        private int RunStockItemImport(AcumaticaStockItemImportContext context, ShopifyVariant variant)
        {
            var matchingShopifySkus = _syncRepository.RetrieveNonMissingVariants(variant.StandardizedSku());

            if (matchingShopifySkus.Count > 1)
            {
                _logService.Log($"Stock Item Import: {variant.LogDescriptor()} has duplicates in Shopify - aborting");
                return StockItemPutResult.NoAction;
            }

            // Attempt to Auto-match
            //
            if (variant.IsMatched())
            {
                _logService.Log($"Stock Item Import: {variant.LogDescriptor()} already matched - aborting");
                return StockItemPutResult.NoAction;
            }

            var stockItem = _syncRepository.RetrieveStockItem(variant.StandardizedSku());
            if (stockItem != null)
            {
                if (stockItem.IsMatched())
                {
                    var msg = $"Stock Item Import: {variant.LogDescriptor()} SKU already synchronized";
                    _logService.Log(msg);
                    return StockItemPutResult.NoAction;
                }
                else
                { 
                    var msg = $"Stock Item Import: auto-matched {stockItem.LogDescriptor()} to {variant.LogDescriptor()}";
                    _logService.Log(msg);

                    _syncRepository.InsertItemSync(variant, stockItem, context.IsSyncEnabled);
                    return StockItemPutResult.Synchronized;
                }
            }

            // Abort any further processing
            if (context.SynchronizeOnly == true)
            {
                return StockItemPutResult.NoAction;
            }


            // With neither duplicates or Auto-matching having succeeded,
            // ... we'll create a new Stock Item in Acumatica
            //
            StockItemPush(context, variant);
            context.VariantsForNextInventoryReceipt.Add(variant);
            return StockItemPutResult.CreatedStockItem;
        }

        public void StockItemPush(AcumaticaStockItemImportContext context, ShopifyVariant variant)
        {
            _logService.Log(LogBuilder.CreateStockItem(variant));

            var newStockItem = BuildNewStockItem(context.WarehouseId, variant);
            var newStockItemJson = newStockItem.SerializeToJson();

            // Push to Acumatica API
            //
            var result = _distributionClient.AddNewStockItem(newStockItemJson);
            var item = result.DeserializeFromJson<StockItem>();

            // Create Monster record
            //
            var newRecord = new AcumaticaStockItem();

            newRecord.ItemId = item.InventoryID.value;

            _acumaticaJsonService.Upsert(
                AcumaticaJsonType.StockItem, item.InventoryID.value, null, item.SerializeToJson());

            newRecord.AcumaticaDescription = item.Description.value;
            newRecord.AcumaticaTaxCategory = item.TaxCategory.value;
            newRecord.IsVariantSynced = false;
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

        public void RunInventoryReceiptImport(string warehouseId, long shopifyProductId, List<ShopifyVariant> variants)
        {
            var inventoryForSyncing = variants.SelectMany(x => x.ShopifyInventoryLevels).ToList();

            // Push Inventory Receipt to Acumatica API
            //
            var receipt = BuildReceipt(warehouseId, inventoryForSyncing);
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


        private StockItem BuildNewStockItem(string warehouseId, ShopifyVariant variant)
        {
            var settings = _settingsRepository.RetrieveSettings();
            var defaultItemClass = settings.AcumaticaDefaultItemClass;
            var defaultPostingClass = settings.AcumaticaDefaultPostingClass;

            var defaultTaxCategory = variant.ShopifyIsTaxable.TaxCategory(settings);
            var shopifyVariant = _shopifyJsonService.RetrieveVariant(variant.ShopifyVariantId);
            var shopifyProduct = _shopifyJsonService.RetrieveProduct(variant.ShopifyProduct.ShopifyProductId);

            var newStockItem = new StockItem();
            newStockItem.InventoryID = variant.StandardizedSku().ToValue();
            newStockItem.Description 
                = Canonizers.StandardizedStockItemTitle(shopifyProduct, shopifyVariant).ToValue();

            newStockItem.DefaultPrice = ((double) shopifyVariant.price).ToValue();
            newStockItem.DefaultWarehouseID = warehouseId.ToValue();

            var dimensionWeight = (double) shopifyVariant.grams.ToAcumaticaOunces();
            // newStockItem.WeightUOM = WeightCalc.AcumaticaUnitsOfMeasure.ToValue();
            newStockItem.DimensionWeight = dimensionWeight.ToValue();
            newStockItem.ItemClass = defaultItemClass.ToValue();
            newStockItem.PostingClass = defaultPostingClass.ToValue();
            newStockItem.TaxCategory = defaultTaxCategory.ToValue();

            return newStockItem;
        }

        private InventoryReceipt BuildReceipt(string warehouseId, List<ShopifyInventoryLevel> inventory)
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
                if (inventoryLevel.ShopifyAvailableQuantity <= 0)
                {
                    continue;
                }

                var variant = inventoryLevel.ShopifyVariant;
                var location = inventoryLevel.ShopifyLocation;
                var stockItemId = variant.AcumaticaStockItemId();

                var unitCogs = (double)inventoryLevel.ShopifyVariant.ShopifyCost;

                var qty = (double) inventoryLevel.ShopifyAvailableQuantity;
                
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
