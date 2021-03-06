﻿using System.Collections.Generic;
using System.Linq;
using Monster.Middle.Config;
using Monster.Middle.Misc.Logging;
using Monster.Middle.Persist.Instance;
using Monster.Middle.Processes.Acumatica.Persist;
using Monster.Middle.Processes.Shopify.Workers;
using Monster.Middle.Processes.Sync.Misc;
using Monster.Middle.Processes.Sync.Model.Inventory;
using Monster.Middle.Processes.Sync.Persist;
using Push.Foundation.Utilities.General;
using Push.Foundation.Utilities.Json;
using Push.Shopify.Api;
using Push.Shopify.Api.Product;


namespace Monster.Middle.Processes.Sync.Workers
{
    public class ShopifyProductVariantPut
    {
        private readonly SyncInventoryRepository _syncInventoryRepository;
        private readonly SettingsRepository _settingsRepository;
        private readonly ExecutionLogService _logService;
        private readonly ProductApi _productApi;
        private readonly ShopifyInventoryGet _shopifyInventoryGet;
        private readonly ShopifyInventoryPut _shopifyInventoryPut;
        private readonly AcumaticaJsonService _acumaticaJsonService;

        public ShopifyProductVariantPut(
                SyncInventoryRepository syncInventoryRepository,
                SettingsRepository settingsRepository,
                ShopifyInventoryGet shopifyInventoryGet, 
                ShopifyInventoryPut shopifyInventoryPut,
                AcumaticaJsonService acumaticaJsonService,
                ExecutionLogService logService,
                ProductApi productApi)
        {
            _syncInventoryRepository = syncInventoryRepository;
            _settingsRepository = settingsRepository;
            _shopifyInventoryGet = shopifyInventoryGet;
            _shopifyInventoryPut = shopifyInventoryPut;
            _acumaticaJsonService = acumaticaJsonService;
            _logService = logService;
            _productApi = productApi;
        }

        public void RunAddToProduct(ShopifyAddVariantImportContext context)
        {
            if (MonsterConfig.Settings.DisableShopifyPut)
            {
                _logService.Log(LogBuilder.ShopifyPutDisabled());
                return;
            }

            // Attempt to auto-match Item Ids SKU's that exists
            //
            AutomatchExistingSkus(context);

            // Creates payload for Shopify API only including valid Variants not in Shopify
            //
            var createVariantList = CleanAndBuildVariantPayload(context.AcumaticaItemIds);
            var shopifyProductRecord = _syncInventoryRepository.RetrieveProduct(context.ShopifyProductId);

            // Add Variants thru Shopify API
            //
            foreach (var createVariant in createVariantList)
            {
                _logService.Log(LogBuilder.CreatingShopifyVariant(createVariant));

                // Invoke Shopify API and create new Variant
                //
                var json = new {variant = createVariant}.SerializeToJson();
                var resultJson = _productApi.AddVariant(context.ShopifyProductId, json);
                var result = resultJson.DeserializeFromJson<VariantParent>();

                // Create Variant Record and Sync
                //
                var variantRecord =
                    _shopifyInventoryGet.CreateNewVariantRecord(shopifyProductRecord.MonsterId, result.variant);
                CreateSyncRecord(createVariant.sku.StandardizedSku(), variantRecord);
            }

            // Need to refresh local cache of Product, so Inventory Levels records reflect current state
            //
            _shopifyInventoryGet.Run(context.ShopifyProductId);

            foreach (var stockItemId in context.AcumaticaItemIds)
            {
                // Update the Inventory data 
                //
                RunInventoryUpdate(stockItemId);
            }
        }

        public void RunNewProduct(ShopifyNewProductImportContext context)
        {
            if (MonsterConfig.Settings.DisableShopifyPut)
            {
                _logService.Log(LogBuilder.ShopifyPutDisabled());
                return;
            }

            var newVariants = CleanAndBuildVariantPayload(context.AcumaticaItemIds);

            var product = new ProductNew()
            {
                title = context.ProductTitle,
                vendor = context.ProductVendor,
                product_type = context.ProductType,
                variants = new List<ShopifyVariantNew>()
            };
            var parent = new { product = product };
            product.variants = newVariants;

            // POST new Product via Shopify API
            //
            var result = _productApi.CreateProduct(parent.SerializeToJson());
            var resultProduct = result.DeserializeFromJson<ProductParent>();
            var shopifyProductId = resultProduct.product.id;

            // Run ShopifyInventoryGet to pull into local cache
            //
            _shopifyInventoryGet.Run(shopifyProductId);
            var productRecord = _syncInventoryRepository.RetrieveProduct(shopifyProductId);

            // Execution Log entry
            //
            var log = LogBuilder.CreatedShopifyProduct(productRecord);
            _logService.Log(log);

            // Create Sync Records for the Variants that were created
            //
            foreach (var newVariant in newVariants)
            {
                var variantRecord = productRecord.ShopifyVariants.FirstOrDefault(x => x.ShopifySku == newVariant.sku);
                CreateSyncRecord(newVariant.sku, variantRecord);
            }

            // Update Inventory data
            //
            foreach (var itemId in context.AcumaticaItemIds)
            {
                RunInventoryUpdate(itemId);
            }
        }


        private void AutomatchExistingSkus(ShopifyAddVariantImportContext context)
        {
            foreach (var itemId in context.AcumaticaItemIds.ToList())
            {
                var existingVariant =
                    _syncInventoryRepository.RetrieveVariantForAutomatch(context.ShopifyProductId, itemId.StandardizedSku());

                if (existingVariant != null)
                {
                    _logService.Log($"Auto-matched {itemId.LogDescriptorItemId()} to {existingVariant.LogDescriptor()}");

                    CreateSyncRecord(itemId.StandardizedSku(), existingVariant);
                    //context.AcumaticaItemIds.Remove(itemId);
                }
            }
        }

        private List<ShopifyVariantNew> CleanAndBuildVariantPayload(List<string> itemIds)
        {
            var settings = _settingsRepository.RetrieveSettings();
            var output = new List<ShopifyVariantNew>();

            foreach (var itemId in itemIds)
            {
                var stockItemRecord = _syncInventoryRepository.RetrieveStockItem(itemId);
                var stockItem = _acumaticaJsonService.RetrieveStockItem(itemId);

                var price = stockItem.DefaultPrice.value;

                var isTaxable = stockItemRecord.IsTaxable(settings);
                if (isTaxable == null)
                {
                    _logService.Log($"{stockItem.TaxCategory} invalid Tax Category for {stockItemRecord.LogDescriptor()}");
                    continue;
                }

                var existsInShopify = _syncInventoryRepository.SkuExistsInShopify(stockItemRecord.ItemId);
                if (existsInShopify)
                {
                    _logService.Log($"Skipping {stockItemRecord.LogDescriptor()} - exists in Shopify already");
                    continue;
                }

                var standardizedSku = stockItemRecord.ItemId.StandardizedSku();
                
                var variant = new ShopifyVariantNew();
                variant.sku = standardizedSku;
                variant.title = stockItem.Description.value;
                variant.taxable = isTaxable.Value;
                variant.grams = stockItem.DimensionWeight.value.ToShopifyGrams();
                variant.price = (decimal)price;
                variant.option1 = standardizedSku;
                variant.inventory_policy = "deny";
                variant.fulfillment_service = "manual";

                output.Add(variant);
            }

            return output;
        }

        private void CreateSyncRecord(string sku, ShopifyVariant variantRecord)
        {
            var stockItem = _syncInventoryRepository.RetrieveStockItem(sku.StandardizedSku());
            stockItem.IsVariantSynced = false;
            stockItem.AcumaticaInventories.ForEach(x => x.IsInventorySynced = false);
            _syncInventoryRepository.InsertItemSync(variantRecord, stockItem, true);
        }

        private void RunInventoryUpdate(long shopifyVariantId)
        {
            var variantRecord = _syncInventoryRepository.RetrieveVariant(shopifyVariantId);
            var itemId = variantRecord.MatchedStockItem().ItemId;
            RunInventoryUpdate(itemId);
        }

        private void RunInventoryUpdate(string itemId)
        {
            var stockItem = _syncInventoryRepository.RetrieveStockItem(itemId);

            _shopifyInventoryPut.UpdatePriceAndCost(stockItem, true);
            _shopifyInventoryPut.UpdateInventoryLevels(stockItem);
        }
    }
}

