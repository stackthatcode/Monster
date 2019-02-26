using System;
using System.Collections.Generic;
using System.Linq;
using Monster.Acumatica.Api;
using Monster.Acumatica.Api.Common;
using Monster.Acumatica.Api.Distribution;
using Monster.Middle.Persist.Multitenant;
using Monster.Middle.Processes.Acumatica.Persist;
using Monster.Middle.Processes.Shopify.Persist;
using Monster.Middle.Processes.Sync.Extensions;
using Monster.Middle.Processes.Sync.Inventory.Model;
using Push.Foundation.Utilities.Json;
using Push.Foundation.Utilities.Logging;
using Push.Shopify.Api.Product;


namespace Monster.Middle.Processes.Sync.Inventory.Workers
{
    public class AcumaticaInventorySync
    {
        private readonly AcumaticaInventoryRepository _inventoryRepository;
        private readonly SyncInventoryRepository _syncRepository;
        private readonly DistributionClient _distributionClient;
        private readonly PreferencesRepository _preferencesRepository;
        private readonly ExecutionLogRepository _executionLogRepository;
        private readonly IPushLogger _logger;

        public AcumaticaInventorySync(
                AcumaticaInventoryRepository inventoryRepository,
                SyncInventoryRepository syncRepository,                    
                DistributionClient distributionClient,
                PreferencesRepository preferencesRepository,
                ExecutionLogRepository executionLogRepository,
                IPushLogger logger)
        {
            _syncRepository = syncRepository;
            _inventoryRepository = inventoryRepository;
            _distributionClient = distributionClient;
            _preferencesRepository = preferencesRepository;
            _logger = logger;
            _executionLogRepository = executionLogRepository;
        }

        public void Run(AcumaticaInventoryImportContext context)
        {
            foreach (var shopifyProductId in context.ShopifyProductIds)
            {
                var variants = _syncRepository.RetrieveUnmatchedVariants(shopifyProductId);

                foreach (var variant in variants)
                {
                    RunStockItemPush(context, variant);
                }

                if (context.CreateInventoryReceipts)
                {
                    RunInventoryReceipt(shopifyProductId);
                }
            }
        }

        public void RunStockItemPush(
                AcumaticaInventoryImportContext context, UsrShopifyVariant variant)
        {            
            var matchingShopifySkus =
                _syncRepository
                    .RetrieveVariantsWithStockItems(variant.StandardizedSku())
                    .ExcludeMissing();

            if (matchingShopifySkus.Count > 1)
            {
                _executionLogRepository.InsertExecutionLog(
                    $"Shopify Variant SKU {variant.ShopifySku} has duplicates in Shopify");
                return;
            }

            // Attempt to Auto-match
            var stockItem = _syncRepository.RetrieveStockItem(variant.StandardizedSku());

            if (stockItem != null && !stockItem.IsMatchedToShopify())
            {
                _executionLogRepository
                    .InsertExecutionLog(
                        $"Auto-matched Stock Item {stockItem.ItemId} " +
                        $"to Shopify Variant {variant.ShopifyVariantId}");

                _syncRepository.InsertItemSync(variant, stockItem, context.IsSyncEnabled);

                return;
            }

            // With neither duplicates or Auto-matching having succeeded,
            // ... we'll create a new Stock Item in Acumatica
            PushStockItem(context, variant);
        }

        public void PushStockItem(
                AcumaticaInventoryImportContext context, UsrShopifyVariant variant)
        {
            var preferences = _preferencesRepository.RetrievePreferences();
            var defaultItemClass = preferences.AcumaticaDefaultItemClass;
            var defaultPostingClass = preferences.AcumaticaDefaultPostingClass;
            var defaultTaxCategory = preferences.AcumaticaTaxCategory;

            var warehouses = _inventoryRepository.RetrieveWarehouses();
            var defaultWarehouseId = warehouses.First().AcumaticaWarehouseId;

            var shopifyVariant = variant.ShopifyVariantJson.DeserializeFromJson<Variant>();
            var shopifyProduct
                = variant
                    .UsrShopifyProduct
                    .ShopifyJson
                    .DeserializeFromJson<Product>();
            
            var newStockItem = new StockItem();
            newStockItem.InventoryID = variant.StandardizedSku().ToValue();
            newStockItem.Description = 
                Standards.StockItemTitle(shopifyProduct, shopifyVariant).ToValue();

            newStockItem.DefaultPrice = ((double)shopifyVariant.price).ToValue();
            newStockItem.DefaultWarehouseID = defaultWarehouseId.ToValue();

            newStockItem.ItemClass = defaultItemClass.ToValue();
            newStockItem.PostingClass = defaultPostingClass.ToValue();
            newStockItem.TaxCategory = defaultTaxCategory.ToValue();

            var newStockItemJson = newStockItem.SerializeToJson();

            // Push to Acumatica API
            var result = _distributionClient.AddNewStockItem(newStockItemJson);
            var item = result.DeserializeFromJson<StockItem>();

            // Create Monster record
            var newStockItemRecord = new UsrAcumaticaStockItem()
            {
                ItemId = item.InventoryID.value,
                AcumaticaJson = item.SerializeToJson(),
                AcumaticaDescription = item.Description.value,
                DateCreated = DateTime.UtcNow,
                LastUpdated = DateTime.UtcNow,
            };

            using (var transaction = _syncRepository.BeginTransaction())
            {
                _inventoryRepository.InsertStockItems(newStockItemRecord);
                _syncRepository.InsertItemSync(variant, newStockItemRecord, context.IsSyncEnabled);

                var log = $"Created Stock Item {item.InventoryID.value} in Acumatica";
                _executionLogRepository.InsertExecutionLog(log);
                transaction.Commit();
            }
        }

        public void RunInventoryReceipt(long shopifyProductId)
        {
            var inventory = _syncRepository.RetrieveInventoryLevels(shopifyProductId);
            var inventoryForSyncing = inventory.WithMatchedVariants();
            
            // Push Inventory Receipt to Acumatica API
            var receipt = BuildReceipt(inventoryForSyncing);

            // Create Inventory Receipt in Acumatica
            var resultJson = _distributionClient.AddInventoryReceipt(receipt.SerializeToJson());
            var resultObject = resultJson.DeserializeFromJson<InventoryReceipt>();
            

            // Create Monster Record
            var monsterReceipt = new UsrAcumaticaInventoryReceipt();
            monsterReceipt.AcumaticaRefNumber = resultObject.ReferenceNbr.value;
            monsterReceipt.AcumaticaJson = resultJson;
            monsterReceipt.IsReleased = false;
            monsterReceipt.DateCreated = DateTime.UtcNow;
            monsterReceipt.LastUpdate = DateTime.UtcNow;

            // No transaction - keep writing until you can't write no mo'!
            _inventoryRepository.InsertInventoryReceipt(monsterReceipt);

            var log = $"Created Inventory Receipt {monsterReceipt.AcumaticaRefNumber} in Acumatica";
            _executionLogRepository.InsertExecutionLog(log);
            
            foreach (var level in inventoryForSyncing)
            {
                _syncRepository.InsertInventoryReceiptSync(level, monsterReceipt);
            }
        }

        private InventoryReceipt BuildReceipt(List<UsrShopifyInventoryLevel> inventory)
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
                var variant = inventoryLevel.UsrShopifyVariant;
                var location = inventoryLevel.UsrShopifyLocation;
                var stockItemId = variant.AcumaticaStockItemId();

                var unitCogs = (double)inventoryLevel.UsrShopifyVariant.ShopifyCost;

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

        public void RunInventoryReceiptsRelease()
        {
            var receipts = _inventoryRepository.RetrieveUnreleasedInventoryReceipts();

            foreach (var receipt in receipts)
            {
                var releaseEntity = ReleaseInventoryReceipt.Build(receipt.AcumaticaRefNumber);

                // Finally, Release the Inventory Receipt
                _distributionClient.ReleaseInventoryReceipt(releaseEntity.SerializeToJson());

                receipt.IsReleased = true;
                _inventoryRepository.SaveChanges();
            }
        }
    }
}
