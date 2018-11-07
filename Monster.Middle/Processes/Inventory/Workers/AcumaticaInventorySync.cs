using System;
using System.Collections.Generic;
using System.Linq;
using Monster.Acumatica.Api;
using Monster.Acumatica.Api.Common;
using Monster.Acumatica.Api.Distribution;
using Monster.Middle.Persist.Multitenant;
using Monster.Middle.Persist.Multitenant.Acumatica;
using Monster.Middle.Persist.Multitenant.Etc;
using Monster.Middle.Persist.Multitenant.Extensions;
using Monster.Middle.Persist.Multitenant.Shopify;
using Push.Foundation.Utilities.Json;
using Push.Foundation.Utilities.Logging;
using Push.Shopify.Api.Product;


namespace Monster.Middle.Processes.Inventory.Workers
{
    public class AcumaticaInventorySync
    {
        private readonly ShopifyInventoryRepository _shopifyInventoryRepository;
        private readonly AcumaticaInventoryRepository _acumaticaInventoryRepository;
        private readonly TenantRepository _tenantRepository;
        private readonly DistributionClient _distributionClient;
        private readonly IPushLogger _logger;

        public AcumaticaInventorySync(
                    ShopifyInventoryRepository shopifyInventoryRepository, 
                    AcumaticaInventoryRepository acumaticaInventoryRepository,
                    DistributionClient distributionClient,
                    TenantRepository tenantRepository,
                    IPushLogger logger)
        {
            _shopifyInventoryRepository = shopifyInventoryRepository;
            _distributionClient = distributionClient;
            _tenantRepository = tenantRepository;
            _logger = logger;
            _acumaticaInventoryRepository = acumaticaInventoryRepository;
        }

        public void Run()
        {
            var variants = _shopifyInventoryRepository.RetrieveVariants();

            foreach (var variant in variants)
            {
                // Attempt to identify duplicates
                // TODO - refactor this into 
                var matchingShopifySkus =
                    _shopifyInventoryRepository
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
                    _acumaticaInventoryRepository
                        .RetreiveStockItem(variant.StandardizedSku());

                if (stockItem != null)
                {
                    if (stockItem.IsMatchedToShopify())
                    {
                        _logger.Info(
                            $"Acumatica Stock Item {stockItem.ItemId} " +
                            $"is already matched to Shopify Variant " +
                            $"{stockItem.UsrShopifyVariant.ShopifyVariantId}");
                        continue;
                    }
                    else
                    {
                        _logger.Info(
                            $"Auto-matching Stock Item {stockItem.ItemId} " +
                            $"to Shopify Variant {variant.ShopifyVariantId}");

                        stockItem.ShopifyVariantMonsterId = variant.MonsterId;
                        _shopifyInventoryRepository.SaveChanges();
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
            var preferences = _tenantRepository.RetrievePreferences();
            var defaultItemClass = preferences.AcumaticaDefaultItemClass;
            var defaultPostingClass = preferences.AcumaticaDefaultPostingClass;

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

            var newStockItemJson = newStockItem.SerializeToJson();
            var result = _distributionClient.AddNewStockItem(newStockItemJson);

            var item = result.DeserializeFromJson<StockItem>();
            var newData = new UsrAcumaticaStockItem()
            {
                ItemId = item.InventoryID.value,
                AcumaticaJson = item.SerializeToJson(),
                ShopifyVariantMonsterId = variant.MonsterId,
                DateCreated = DateTime.UtcNow,
                LastUpdated = DateTime.UtcNow,
            };

            _acumaticaInventoryRepository.InsertStockItems(newData);
        }

        public void RunInventoryReceipts()
        {            
            var preferences = _tenantRepository.RetrievePreferences();

            if (preferences.AcumaticaPostingDate == null)
                throw new ArgumentException(
                    "Preferences -> AcumaticaPostDate is not set");

            if (preferences.DefaultCoGsMargin == null)
                throw new ArgumentException(
                    "Preferences -> DefaultCoGsMargin is not set");

            var inventory =
                _shopifyInventoryRepository
                    .RetrieveInventoryLevelsMatchedButNotSynced();

            var productParentMonsterIds =
                inventory
                    .Select(x => x.UsrShopifyVariant.ParentMonsterId)
                    .Distinct()
                    .ToList();

            foreach (var parentMonsterId in productParentMonsterIds)
            {
                var inventoryByProduct = inventory.ByParentId(parentMonsterId);

                // Push to Acumatica
                //
                var receipt = BuildReceipt(inventoryByProduct);

                var resultJson =
                    _distributionClient
                        .AddInventoryReceipt(receipt.SerializeToJson());

                var resultObject
                    = resultJson.DeserializeFromJson<InventoryReceipt>();


                // Save to persistence
                //
                var monsterReceipt = new UsrAcumaticaInventoryReceipt();
                monsterReceipt.AcumaticaRefNumber = resultObject.ReferenceNbr.value;
                monsterReceipt.AcumaticaJson = resultJson;
                monsterReceipt.IsReleased = false;
                monsterReceipt.DateCreated = DateTime.UtcNow;
                monsterReceipt.LastUpdate = DateTime.UtcNow;

                // TODO - add logging in case this blows us during persistence

                _acumaticaInventoryRepository
                    .InsertInventoryReceipt(monsterReceipt);

                _acumaticaInventoryRepository
                    .UpdateInventoryReceipt(
                        inventoryByProduct, monsterReceipt);
            }
        }

        public void RunInventoryReceiptsRelease()
        {
            var receipts = 
                _acumaticaInventoryRepository
                    .RetrieveUnreleasedInventoryReceipts();

            foreach (var receipt in receipts)
            {
                var releaseEntity
                    = ReleaseInventoryReceipt.Build(receipt.AcumaticaRefNumber);

                // Finally, Release the Inventory Receipt
                _distributionClient.ReleaseInventoryReceipt(releaseEntity.SerializeToJson());

                receipt.IsReleased = true;
                _shopifyInventoryRepository.SaveChanges();
            }
        }

        private InventoryReceipt BuildReceipt(List<UsrShopifyInventoryLevel> inventory)
        {
            var preferences = _tenantRepository.RetrievePreferences();
            var postingDate = preferences.AcumaticaPostingDate.Value;
            var defaultCogs = preferences.DefaultCoGsMargin.Value;

            var controlQty
                = inventory.Sum(x => x.ShopifyAvailableQuantity);

            // TODO - move to extension method
            var controlCost
                = inventory.Sum(
                    x => 
                        x.UsrShopifyVariant.CogsByMarginPercent(defaultCogs) 
                        * x.ShopifyAvailableQuantity);

            var receipt = new InventoryReceipt();
            receipt.Date = postingDate.ToValue();
            receipt.ControlCost = controlCost.ToValue();
            receipt.ControlQty = ((double)controlQty).ToValue();
            receipt.Details = new List<InventoryReceiptDetails>();
            receipt.Hold = false.ToValue();

            foreach (var inventoryLevel in inventory)
            {
                var stockItemId = inventoryLevel.AcumaticaStockItemId();
                var unitCogs =
                    inventoryLevel.UsrShopifyVariant.CogsByMarginPercent(defaultCogs);

                var qty = (double) inventoryLevel.ShopifyAvailableQuantity;
                var warehouseId = inventoryLevel.AcumaticaWarehouseId();


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
