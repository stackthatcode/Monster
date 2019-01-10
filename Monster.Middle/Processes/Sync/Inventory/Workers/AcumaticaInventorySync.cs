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
using Monster.Middle.Services;
using Push.Foundation.Utilities.Json;
using Push.Foundation.Utilities.Logging;
using Push.Shopify.Api.Product;


namespace Monster.Middle.Processes.Sync.Inventory.Workers
{
    public class AcumaticaInventorySync
    {
        private readonly AcumaticaInventoryRepository _inventoryRepository;
        private readonly SyncInventoryRepository _syncInventoryRepository;
        private readonly ConnectionRepository _connectionRepository;
        private readonly InstanceTimeZoneService _instanceTimeZoneService;
        private readonly DistributionClient _distributionClient;
        private readonly IPushLogger _logger;
        private readonly PreferencesRepository _preferencesRepository;

        public AcumaticaInventorySync(
                    AcumaticaInventoryRepository inventoryRepository,
                    SyncInventoryRepository syncInventoryRepository,
                    ConnectionRepository connectionRepository,
                    
                    DistributionClient distributionClient,
                    InstanceTimeZoneService instanceTimeZoneService,
                    IPushLogger logger, 
                    PreferencesRepository preferencesRepository)
        {

            _connectionRepository = connectionRepository;
            _syncInventoryRepository = syncInventoryRepository;
            _inventoryRepository = inventoryRepository;

            _distributionClient = distributionClient;
            _logger = logger;
            _preferencesRepository = preferencesRepository;
            _instanceTimeZoneService = instanceTimeZoneService;
        }

        public void Run()
        {
            var variants = _syncInventoryRepository.RetrieveVariants();

            foreach (var variant in variants)
            {
                // Attempt to identify duplicates
                // TODO - refactor this into 
                var matchingShopifySkus =
                    _syncInventoryRepository
                        .RetrieveVariantsWithStockItems(variant.StandardizedSku())
                        .ExcludeMissing()
                        .ExcludeMatched();
                
                if (matchingShopifySkus.Count > 1)
                {
                    _logger.Info("Shopify Variant: " + 
                        $"{variant.ShopifyVariantId}/{variant.ShopifySku} has duplicates");
                    continue;
                }

                // Attempt to Auto-match
                var stockItem =
                    _syncInventoryRepository.RetrieveStockItem(variant.StandardizedSku());

                if (stockItem != null)
                {
                    if (stockItem.IsMatchedToShopify())
                    {
                        _logger.Info(
                            $"Acumatica Stock Item {stockItem.ItemId} " +
                            $"is already matched to Shopify Variant " +
                            $"{stockItem.MatchedVariant().ShopifyVariantId}");
                        continue;
                    }
                    else
                    {
                        _logger.Info(
                            $"Auto-matching Stock Item {stockItem.ItemId} " +
                            $"to Shopify Variant {variant.ShopifyVariantId}");

                        _syncInventoryRepository.InsertItemSync(variant, stockItem);
                        continue;
                    }
                }

                // With neither duplicates or Auto-matching having succeeded,
                // ... we'll create a new Stock Item in Acumatica
                PushStockItem(variant);
            }
        }

        public void PushStockItem(UsrShopifyVariant variant)
        {
            var preferences = _preferencesRepository.RetrievePreferences();
            var defaultItemClass = preferences.AcumaticaDefaultItemClass;
            var defaultPostingClass = preferences.AcumaticaDefaultPostingClass;
            var defaultTaxCategory = preferences.AcumaticaTaxCategory;

            var shopifyVariant
                = variant.ShopifyVariantJson.DeserializeFromJson<Variant>();

            var shopifyProduct
                = variant
                    .UsrShopifyProduct
                    .ShopifyJson
                    .DeserializeFromJson<Product>();
            
            var newStockItem = new StockItem();
            newStockItem.InventoryID = variant.StandardizedSku().ToValue();
            newStockItem.Description =
                Standards.StockItemTitle(
                    shopifyProduct, shopifyVariant).ToValue();

            newStockItem.DefaultPrice 
                    = ((double)shopifyVariant.price).ToValue();

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
                DateCreated = DateTime.UtcNow,
                LastUpdated = DateTime.UtcNow,
            };

            _inventoryRepository.InsertStockItems(newStockItemRecord);
            _syncInventoryRepository.InsertItemSync(variant, newStockItemRecord);
        }

        public void RunInventoryReceipts()
        {
            // ON HOLD
            //var preferences = _connectionRepository.RetrievePreferences();
            //if (preferences.DefaultCoGsMargin == null)
            //    throw new ArgumentException(
            //        "Preferences -> DefaultCoGsMargin is not set");

            var inventory =
                _syncInventoryRepository.RetrieveInventoryLevelsNotSynced();

            var matchedInventory =
                inventory
                    .Where(x => x.UsrShopifyVariant.IsMatched())
                    .ToList();

            var productParentMonsterIds =
                matchedInventory
                    .Select(x => x.UsrShopifyVariant.ParentMonsterId)
                    .Distinct()
                    .ToList();

            foreach (var parentMonsterId in productParentMonsterIds)
            {
                var inventoryByProduct = inventory.ByParentId(parentMonsterId);

                // Push Inventory Receipt to Acumatica API
                var receipt = BuildReceipt(inventoryByProduct);

                var resultJson =
                    _distributionClient
                        .AddInventoryReceipt(receipt.SerializeToJson());

                var resultObject
                    = resultJson.DeserializeFromJson<InventoryReceipt>();
                
                // Create Monster Record
                var monsterReceipt = new UsrAcumaticaInventoryReceipt();
                monsterReceipt.AcumaticaRefNumber = resultObject.ReferenceNbr.value;
                monsterReceipt.AcumaticaJson = resultJson;
                monsterReceipt.IsReleased = false;
                monsterReceipt.DateCreated = DateTime.UtcNow;
                monsterReceipt.LastUpdate = DateTime.UtcNow;
                
                _inventoryRepository
                    .InsertInventoryReceipt(monsterReceipt);

                foreach (var level in inventoryByProduct)
                {
                    _syncInventoryRepository
                        .InsertInventoryReceiptSync(level, monsterReceipt);
                }
            }
        }

        public void RunInventoryReceiptsRelease()
        {
            var receipts = 
                _inventoryRepository
                    .RetrieveUnreleasedInventoryReceipts();

            foreach (var receipt in receipts)
            {
                var releaseEntity
                    = ReleaseInventoryReceipt.Build(receipt.AcumaticaRefNumber);

                // Finally, Release the Inventory Receipt
                _distributionClient.ReleaseInventoryReceipt(releaseEntity.SerializeToJson());

                receipt.IsReleased = true;
                _inventoryRepository.SaveChanges();
            }
        }

        private InventoryReceipt BuildReceipt(
                    List<UsrShopifyInventoryLevel> inventory)
        {
            // TODO - determine is this is required
            //var preferences = _connectionRepository.RetrievePreferences();
            //var defaultCogs = preferences.DefaultCoGsMargin.Value;

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

                // TODO - determine if this type of function is useful or necessary
                //CogsByMarginPercent(defaultCogs);

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
