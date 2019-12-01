using System;
using System.Collections.Generic;
using System.Linq;
using Monster.Acumatica.Api.Distribution;
using Monster.Middle.Misc.Logging;
using Monster.Middle.Persist.Instance;
using Monster.Middle.Processes.Shopify.Workers;
using Monster.Middle.Processes.Sync.Misc;
using Monster.Middle.Processes.Sync.Model.Inventory;
using Monster.Middle.Processes.Sync.Model.Orders;
using Monster.Middle.Processes.Sync.Persist;
using Push.Foundation.Utilities.General;
using Push.Foundation.Utilities.Helpers;
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

        public ShopifyProductVariantPut(
                SyncInventoryRepository syncInventoryRepository,
                SettingsRepository settingsRepository,
                ShopifyInventoryGet shopifyInventoryGet, 
                ShopifyInventoryPut shopifyInventoryPut,
                ExecutionLogService logService,
                ProductApi productApi)
        {
            _syncInventoryRepository = syncInventoryRepository;
            _settingsRepository = settingsRepository;
            _shopifyInventoryGet = shopifyInventoryGet;
            _shopifyInventoryPut = shopifyInventoryPut;
            _logService = logService;
            _productApi = productApi;
        }

        public void Run(ShopifyAddVariantImportContext context)
        {
            // Attempt to auto-match Item Ids SKU's that exists, and remove from context
            //
            AutomatchExistingSkus(context);

            // Build the payload to send to Shopify API
            //
            var product = new ProductVariantUpdate();
            product.id = context.ShopifyProductId;
            product.variants = new List<VariantNew>();
            var parent = new { product };

            var newVariants = BuildValidVariantList(context.AcumaticaItemIds);
            product.variants = newVariants;

            // PUT update to Product via Shopify API
            //
            var result = _productApi.Update(context.ShopifyProductId, parent.SerializeToJson());
            var resultProduct = result.DeserializeFromJson<ProductParent>();
            var shopifyProductId = resultProduct.product.id;

            // Run ShopifyInventoryGet to pull into local cache
            //
            _shopifyInventoryGet.Run(shopifyProductId);
            var updatedProductRecord = _syncInventoryRepository.RetrieveProduct(shopifyProductId);

            // Create Sync Records
            //
            WriteSyncRecords(newVariants, updatedProductRecord);

            // Update Inventory data
            //
            RunInventoryUpdate(context.AcumaticaItemIds);
        }

        public void Run(ShopifyNewProductImportContext context)
        {
            var product = new ProductNew()
            {
                title = context.ProductTitle,
                vendor = context.ProductVendor,
                product_type = context.ProductType,
                variants = new List<VariantNew>()
            };
            var parent = new { product = product };

            var newVariants = BuildValidVariantList(context.AcumaticaItemIds);
            product.variants = newVariants;

            // POST new Product via Shopify API
            //
            var result = _productApi.Create(parent.SerializeToJson());
            var resultProduct = result.DeserializeFromJson<ProductParent>();
            var shopifyProductId = resultProduct.product.id;

            // Run ShopifyInventoryGet to pull into local cache
            //
            _shopifyInventoryGet.Run(shopifyProductId);
            var productRecord = _syncInventoryRepository.RetrieveProduct(shopifyProductId);

            var log = LogBuilder.CreatedShopifyProduct(productRecord);
            _logService.Log(log);

            // Create Sync Records for the Variants that were created
            //
            WriteSyncRecords(newVariants, productRecord);

            // Update Inventory data
            //
            RunInventoryUpdate(context.AcumaticaItemIds);
        }

        private void AutomatchExistingSkus(ShopifyAddVariantImportContext context)
        {
            foreach (var itemId in context.AcumaticaItemIds)
            {
                var existingVariant =
                    _syncInventoryRepository.RetrieveLiveVariant(context.ShopifyProductId, itemId.StandardizedSku());

                if (existingVariant != null)
                {
                    _logService.Log(
                        $"Auto-matched {itemId.LogDescriptorItemId()} to {existingVariant.LogDescriptor()}");

                    WriteSyncRecord(itemId.StandardizedSku(), existingVariant);
                    context.AcumaticaItemIds.Remove(itemId);
                }
            }
        }

        private List<VariantNew> BuildValidVariantList(List<string> itemIds)
        {
            var settings = _settingsRepository.RetrieveSettings();
            var output = new List<VariantNew>();

            foreach (var itemId in itemIds)
            {
                var stockItemRecord = _syncInventoryRepository.RetrieveStockItem(itemId);
                var stockItem = stockItemRecord.AcumaticaJson.DeserializeFromJson<StockItem>();
                var price = stockItem.DefaultPrice.value;

                var isTaxable = stockItemRecord.IsTaxable(settings);
                if (isTaxable == null)
                {
                    throw new Exception(
                        $"{stockItem.TaxCategory} invalid Tax Category for {stockItemRecord.ItemId}");
                }

                var existsInShopify = _syncInventoryRepository.SkuExistsInShopify(stockItemRecord.ItemId);
                if (existsInShopify)
                {
                    _logService.Log($"Skipping {stockItemRecord.LogDescriptor()} - exists in Shopify already");
                    continue;
                }

                var shopifyWeightG = stockItem.DimensionWeight.value.ToShopifyGrams();

                var variant = new VariantNew();
                variant.sku = stockItemRecord.ItemId;
                variant.option1 = $"OPTION1";
                variant.taxable = isTaxable.Value;
                variant.grams = shopifyWeightG;
                variant.price = (decimal)price;
                variant.inventory_policy = "deny";
                variant.fulfillment_service = "shopify";

                _logService.Log(LogBuilder.CreateShopifyVariant(stockItemRecord));
                output.Add(variant);
            }

            return output;
        }

        private void WriteSyncRecords(List<VariantNew> newVariants, ShopifyProduct productRecord)
        {
            foreach (var newVariant in newVariants)
            {
                var variantRecord = productRecord.ShopifyVariants.FirstOrDefault(x => x.ShopifySku == newVariant.sku);
                WriteSyncRecord(newVariant.sku, variantRecord);
            }
        }

        private void WriteSyncRecord(string sku, ShopifyVariant variantRecord)
        {
            var stockItem = _syncInventoryRepository.RetrieveStockItem(sku.StandardizedSku());
            stockItem.IsPriceSynced = false;
            stockItem.AcumaticaInventories.ForEach(x => x.IsInventorySynced = false);
            _syncInventoryRepository.InsertItemSync(variantRecord, stockItem, true);
        }

        private void RunInventoryUpdate(List<string> acumaticaStockItemIds)
        {
            foreach (var itemId in acumaticaStockItemIds)
            {
                var stockItem = _syncInventoryRepository.RetrieveStockItem(itemId);

                _shopifyInventoryPut.PriceAndCostUpdate(stockItem);
                _shopifyInventoryPut.InventoryUpdate(stockItem);
            }
        }
    }
}

