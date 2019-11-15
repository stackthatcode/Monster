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

        public ShopifyProductVariantPut(
                SyncInventoryRepository syncInventoryRepository,
                SettingsRepository settingsRepository,
                ExecutionLogService logService, 
                ProductApi productApi,
                ShopifyInventoryGet shopifyInventoryGet)
        {
            _syncInventoryRepository = syncInventoryRepository;
            _settingsRepository = settingsRepository;
            _logService = logService;
            _productApi = productApi;
            _shopifyInventoryGet = shopifyInventoryGet;
        }

        public void Run(ShopifyAddVariantImportContext context)
        {
            var product = new ProductVariantUpdate();
            product.id = context.ShopifyProductId;
            product.variants = new List<VariantNew>();
            var parent = new { product };

            var stockItemRecords = BuildShopifyVariants(context.AcumaticaItemIds, product.variants);

            var result = _productApi.Update(context.ShopifyProductId, parent.SerializeToJson());
            var resultProduct = result.DeserializeFromJson<ProductParent>();
            var shopifyProductId = resultProduct.product.id;

            _shopifyInventoryGet.Run(shopifyProductId);
            var productRecord = _syncInventoryRepository.RetrieveProduct(shopifyProductId);

            WriteSyncRecords(stockItemRecords, productRecord);
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

            var stockItemRecords = BuildShopifyVariants(context.AcumaticaItemIds, product.variants);

            var result = _productApi.Create(parent.SerializeToJson());
            var resultProduct = result.DeserializeFromJson<ProductParent>();
            var shopifyProductId = resultProduct.product.id;

            _shopifyInventoryGet.Run(shopifyProductId);
            var productRecord = _syncInventoryRepository.RetrieveProduct(shopifyProductId);
            LogBuilder.CreatedShopifyProduct(productRecord);

            WriteSyncRecords(stockItemRecords, productRecord);
        }

        private void WriteSyncRecords(List<AcumaticaStockItem> stockItemRecords, ShopifyProduct productRecord)
        {
            foreach (var stockItem in stockItemRecords)
            {
                var variantRecord = productRecord.ShopifyVariants.FirstOrDefault(x => x.ShopifySku == stockItem.ItemId);
                _syncInventoryRepository.InsertItemSync(variantRecord, stockItem, true);
            }
        }

        private List<AcumaticaStockItem> BuildShopifyVariants(List<string> itemIds, List<VariantNew> variants)
        {
            var settings = _settingsRepository.RetrieveSettings();
            var stockItemRecords = GetStockItems(itemIds);

            var counter = 1;

            foreach (var stockItemRecord in stockItemRecords)
            {
                var stockItem = stockItemRecord.AcumaticaJson.DeserializeFromJson<StockItem>();
                var price = stockItem.DefaultPrice.value;

                var isTaxable = stockItemRecord.IsTaxable(settings);
                if (isTaxable == null)
                {
                    throw new Exception(
                        $"{stockItem.TaxCategory} invalid Tax Category for {stockItemRecord.ItemId}");
                }

                var variant = new VariantNew();
                variant.sku = stockItemRecord.ItemId;
                variant.option1 = $"OPTION{counter++}";

                //variant.title 
                //    = stockItemRecord
                //        .AcumaticaDescription
                //        .IsNullOrEmptyAlt(stockItemRecord.ItemId);

                variant.taxable = isTaxable.Value;
                variant.price = (decimal)price;
                variant.inventory_policy = "deny";
                variant.fulfillment_service = "manual";

                _logService.Log(LogBuilder.CreateShopifyVariant(stockItemRecord));
                variants.Add(variant);
            }

            return stockItemRecords;
        }

        public List<AcumaticaStockItem> GetStockItems(List<string> itemIds)
        {
            return itemIds.Select(x => _syncInventoryRepository.RetrieveStockItem(x)).ToList();
        }
    }
}
