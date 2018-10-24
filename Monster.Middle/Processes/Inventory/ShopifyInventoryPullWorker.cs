using System;
using System.Collections.Generic;
using System.Linq;
using Monster.Middle.Persist.Multitenant;
using Monster.Middle.Persist.Multitenant.Extensions;
using Push.Foundation.Utilities.General;
using Push.Foundation.Utilities.Json;
using Push.Foundation.Utilities.Logging;
using Push.Shopify.Api;
using Push.Shopify.Api.Product;


namespace Monster.Middle.Processes.Inventory
{
    public class ShopifyInventoryPullWorker
    {
        private readonly ProductApi _productApi;
        private readonly InventoryApi _inventoryApi;
        private readonly InventoryRepository _inventoryRepository;
        private readonly BatchStateRepository _batchStateRepository;
        private readonly IPushLogger _logger;

        // Possibly expand - this is a one-time thing...
        public const int InitialBatchStateFudgeMin = -15;


        public ShopifyInventoryPullWorker(
                ProductApi productApi,
                InventoryApi inventoryApi,
                InventoryRepository inventoryRepository, 
                BatchStateRepository batchStateRepository,
                IPushLogger logger)
        {
            _productApi = productApi;
            _inventoryApi = inventoryApi;
            _inventoryRepository = inventoryRepository;
            _batchStateRepository = batchStateRepository;
            _logger = logger;
        }

        public void BaselinePull()
        {
            _logger.Debug("Baseline Pull Products");

            _batchStateRepository.ResetBatchState();

            var firstFilter = new ProductFilter();
            firstFilter.Page = 1;

            var firstJson = _productApi.Retrieve(firstFilter);
            var firstProducts = firstJson.DeserializeFromJson<ProductList>().products;
            UpsertProductsAndInventory(firstProducts);

            var currentPage = 2;

            while (true)
            {
                var currentFilter = firstFilter.Clone();
                currentFilter.Page = currentPage;
                
                var currentJson = _productApi.Retrieve(currentFilter);
                var currentProducts = currentJson.DeserializeFromJson<ProductList>().products;
                UpsertProductsAndInventory(currentProducts);

                if (currentProducts.Count == 0)
                {
                    break;
                }

                currentPage++;
            }
            
            var maxUpdatedDate = 
                _inventoryRepository.RetrieveShopifyProductMaxUpdatedDate();

            var productBatchEnd
                = maxUpdatedDate
                  ?? DateTime.UtcNow.AddMinutes(InitialBatchStateFudgeMin);

            _batchStateRepository.UpdateShopifyProductsPullEnd(productBatchEnd);            
        }

        public void DifferentialPull()
        {
            var batchState = _batchStateRepository.RetrieveBatchState();

            if (!batchState.ShopifyProductsPullEnd.HasValue)
            {
                throw new Exception(
                    "ShopifyProductsEndDate not set - must run Baseline Pull first");
            }

            var filterMinUpdated = batchState.ShopifyProductsPullEnd.Value;
            var startOfPullRun = DateTime.UtcNow; // Trick - we won't use this in filtering

            var firstFilter = new ProductFilter();
            firstFilter.Page = 1;
            firstFilter.UpdatedAtMinUtc = filterMinUpdated;

            // Pull from Shopify
            var firstJson = _productApi.Retrieve(firstFilter);
            var firstProducts = firstJson.DeserializeFromJson<ProductList>().products;
            UpsertProductsAndInventory(firstProducts);

            var currentPage = 2;

            while (true)
            {
                var currentFilter = firstFilter.Clone();
                currentFilter.Page = currentPage;

                // Pull from Shopify
                var currentJson = _productApi.Retrieve(currentFilter);
                var currentProducts = currentJson.DeserializeFromJson<ProductList>().products;
                UpsertProductsAndInventory(currentProducts);

                if (currentProducts.Count == 0)
                {
                    break;
                }

                currentPage++;
            }
            
            _batchStateRepository.UpdateShopifyProductsPullEnd(startOfPullRun);
        }


        public void UpsertProductsAndInventory(IEnumerable<Product> products)
        {
            products.ForEach(x => UpsertProductAndInventory(x));
        }

        public void UpsertProductAndInventory(Product product)
        {
            var existing =
                _inventoryRepository.RetrieveShopifyProduct(product.id);

            long productMonsterId;
                
            if (existing == null)
            {
                var data = new UsrShopifyProduct
                {
                    ShopifyProductId = product.id,
                    ShopifyJson = product.SerializeToJson(),
                    DateCreated = DateTime.UtcNow,
                    LastUpdated = DateTime.UtcNow,
                };

                _inventoryRepository.InsertShopifyProduct(data);
                _inventoryRepository.SaveChanges();

                productMonsterId = data.MonsterId;
            }
            else
            {
                existing.ShopifyJson = product.SerializeToJson();
                existing.LastUpdated = DateTime.UtcNow;
                _inventoryRepository.SaveChanges();

                productMonsterId = existing.MonsterId;
            }

            // Write the Product Variants
            UpsertVariants(productMonsterId, product);

            // Flags the missing Variants
            FlagMissingVariants(productMonsterId, product);

            // Pull and write the Inventory
            PullAndUpsertInventory(productMonsterId);
        }

        public void UpsertVariants(long parentMonsterId, Product product)
        {
            foreach (var variant in product.variants)
            {
                UpsertVariant(parentMonsterId, variant);
            }
        }

        public void UpsertVariant(long parentProductId, Variant variant)
        {
            var existing = 
                _inventoryRepository.RetrieveShopifyVariant(variant.id, variant.sku);
            
            if (existing == null)
            {
                var data = new UsrShopifyVariant
                {
                    ParentMonsterId = parentProductId,
                    ShopifyVariantId = variant.id,
                    ShopifySku = variant.sku,
                    ShopifyInventoryItemId = variant.inventory_item_id,
                    ShopifyVariantJson = variant.SerializeToJson(),
                    IsMissing = false,
                    DateCreated = DateTime.UtcNow,
                    LastUpdated = DateTime.UtcNow,
                };
                
                _inventoryRepository.InsertShopifyVariant(data);
            }
            else
            {
                existing.ShopifyVariantJson = variant.SerializeToJson();
                existing.LastUpdated = DateTime.UtcNow;

                _inventoryRepository.SaveChanges();
            }
        }

        public void FlagMissingVariants(long parentMonsterId, Product product)
        {
            var variants = 
                _inventoryRepository.RetrieveShopifyVariantsByParent(parentMonsterId);

            foreach (var variant in variants)
            {
                if (product.variants.Any(x => x.id == variant.ShopifyVariantId))
                {
                    break;
                }

                _logger.Info($"Shopify Variant {variant.ShopifyVariantId}/{variant.ShopifySku} " +
                             "appears to be missing - flagged");

                variant.IsMissing = true;
                _inventoryRepository.SaveChanges();
            }
        }

        public void PullAndUpsertInventory(long parentMonsterId)
        {
            var variants =
                _inventoryRepository
                    .RetrieveShopifyVariantsByParent(parentMonsterId)
                    .ExcludeMissing();
            
            var inventoryItemIds
                = variants.Select(x => x.ShopifyInventoryItemId).ToList();

            var inventoryJson =
                _inventoryApi.RetrieveInventoryItems(inventoryItemIds);

            var inventoryLevels
                = inventoryJson
                    .DeserializeFromJson<InventoryLevelList>()
                    .inventory_levels;

            foreach (var variant in variants)
            {
                var variantLevels =
                    inventoryLevels
                        .Where(x => x.inventory_item_id == variant.ShopifyInventoryItemId)
                        .ToList();

                UpsertInventory(variant, variantLevels);
            }
        }

        public void UpsertInventory(
                    UsrShopifyVariant variant, List<InventoryLevel> shopifyLevels)
        {
            var existingLevels =
                _inventoryRepository
                    .RetrieveShopifyInventoryLevels(variant.ShopifyInventoryItemId);

            foreach (var shopifyLevel in shopifyLevels)
            {
                var existingLevel =
                    existingLevels.FirstOrDefault(x => x.ShopifyLocationId == shopifyLevel.location_id);

                if (existingLevel == null)
                {
                    var newLevel = new UsrShopifyInventoryLevel();
                    newLevel.ParentMonsterId = variant.MonsterId;
                    newLevel.ShopifyInventoryItemId = shopifyLevel.inventory_item_id;
                    newLevel.ShopifyLocationId = shopifyLevel.location_id;
                    newLevel.ShopifyAvailableQuantity = shopifyLevel.available ?? 0;
                    newLevel.DateCreated = DateTime.UtcNow;
                    newLevel.LastUpdated = DateTime.UtcNow;

                    _inventoryRepository.InsertShopifyInventoryLevel(newLevel);
                }
                else
                {
                    existingLevel.ShopifyAvailableQuantity = shopifyLevel.available ?? 0;
                    existingLevel.LastUpdated = DateTime.UtcNow;
                }
            }
        }
    }
}
