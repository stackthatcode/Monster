using System;
using System.Collections.Generic;
using System.Linq;
using Monster.Middle.Persist.Multitenant;
using Monster.Middle.Processes.Shopify.Persist;
using Push.Foundation.Utilities.General;
using Push.Foundation.Utilities.Json;
using Push.Foundation.Utilities.Logging;
using Push.Shopify.Api;
using Push.Shopify.Api.Event;
using Push.Shopify.Api.Inventory;
using Push.Shopify.Api.Product;

namespace Monster.Middle.Processes.Shopify.Workers
{
    public class ShopifyInventoryPull
    {
        private readonly ProductApi _productApi;
        private readonly InventoryApi _inventoryApi;
        private readonly EventApi _eventApi;
        private readonly ShopifyInventoryRepository _inventoryRepository;
        private readonly ShopifyBatchRepository _batchRepository;
        private readonly IPushLogger _logger;
        

        public ShopifyInventoryPull(
                IPushLogger logger,
                ProductApi productApi,
                InventoryApi inventoryApi,
                EventApi eventApi,
                ShopifyInventoryRepository inventoryRepository, 
                ShopifyBatchRepository batchRepository)
        {
            _productApi = productApi;
            _inventoryApi = inventoryApi;
            _eventApi = eventApi;
            _inventoryRepository = inventoryRepository;
            _batchRepository = batchRepository;
            _logger = logger;
        }

        public void RunAutomatic()
        {
            var batchState = _batchRepository.Retrieve();
            if (batchState.ShopifyProductsPullEnd.HasValue)
            {
                RunUpdated();
            }
            else
            {
                RunAll();
            }
        }

        private void RunAll()
        {
            _logger.Debug("ShopifyInventoryPull -> RunAll()");

            // We've hanging on to this to compute the end of the Batch State
            var startOfRun = DateTime.UtcNow;

            var firstFilter = new SearchFilter();
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

            // Process Delete Events
            PullDeletedEventsAndUpsert(startOfRun);

            // Compute the Batch State end marker
            var maxUpdatedDate = _inventoryRepository.RetrieveProductMaxUpdatedDate();
            var batchEnd = (maxUpdatedDate ?? startOfRun).AddBatchFudge();
                        
            _batchRepository.UpdateProductsPullEnd(batchEnd);            
        }

        private void RunUpdated()
        {
            _logger.Debug("ShopifyInventoryPull -> RunUpdated()");

            var startOfRun = DateTime.UtcNow;
            var batchState = _batchRepository.Retrieve();

            if (!batchState.ShopifyProductsPullEnd.HasValue)
            {
                throw new Exception(
                    "ShopifyProductsEndDate not set - must run Baseline Pull first");
            }

            var lastPullEnd = batchState.ShopifyProductsPullEnd.Value;
            
            var firstFilter = new SearchFilter();
            firstFilter.UpdatedAtMinUtc = lastPullEnd;
            firstFilter.Page = 1;

            // Pull from Shopify
            var firstJson = _productApi.Retrieve(firstFilter);
            var firstProducts = firstJson.DeserializeFromJson<ProductList>().products;
            UpsertProductsAndInventory(firstProducts);

            var currentPage = 2;

            while (true)
            {
                var currentFilter = new SearchFilter();
                currentFilter.UpdatedAtMinUtc = lastPullEnd;
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

            // Get all of the Delete Events
            PullDeletedEventsAndUpsert(lastPullEnd);

            _batchRepository.UpdateProductsPullEnd(startOfRun);
        }

        public long Run(long shopifyProductId)
        {
            var productJson = _productApi.Retrieve(shopifyProductId);
            var product = productJson.DeserializeFromJson<ProductParent>();
            return UpsertProductAndInventory(product.product);
        }

        public void UpsertProductsAndInventory(IEnumerable<Product> products)
        {
            products.ForEach(x => UpsertProductAndInventory(x));
        }

        public long UpsertProductAndInventory(Product product)
        {
            var existing =
                _inventoryRepository.RetrieveProduct(product.id);

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

                _inventoryRepository.InsertProduct(data);
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

            return productMonsterId;
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
                _inventoryRepository.RetrieveVariant(variant.id, variant.sku);
            
            if (existing == null)
            {
                var data = new UsrShopifyVariant();

                data.ParentMonsterId = parentProductId;
                data.ShopifyVariantId = variant.id;
                data.ShopifySku = variant.sku;
                data.ShopifyInventoryItemId = variant.inventory_item_id;
                data.ShopifyVariantJson = variant.SerializeToJson();
                data.ShopifyIsTracked = variant.IsTracked;
                data.IsMissing = false;
                data.DateCreated = DateTime.UtcNow;
                data.LastUpdated = DateTime.UtcNow;
                
                _inventoryRepository.InsertVariant(data);
            }
            else
            {
                existing.ShopifyVariantJson = variant.SerializeToJson();
                existing.ShopifyIsTracked = variant.IsTracked;
                existing.LastUpdated = DateTime.UtcNow;

                _inventoryRepository.SaveChanges();
            }
        }

        
        public void FlagMissingVariants(long parentMonsterId, Product product)
        {
            var storedVariants = 
                _inventoryRepository.RetrieveVariantsByParent(parentMonsterId);

            foreach (var variant in storedVariants)
            {
                if (product.variants.Any(
                        x => x.id == variant.ShopifyVariantId &&
                             x.sku == variant.ShopifySku))
                {
                    break;
                }

                _logger.Debug($"Shopify Variant {variant.ShopifyVariantId}/{variant.ShopifySku} " +
                             "appears to be missing - flagged");

                variant.IsMissing = true;
                _inventoryRepository.SaveChanges();
            }
        }

        public void PullAndUpsertInventory(long productMonsterId)
        {
            var variants =
                _inventoryRepository
                    .RetrieveVariantsByParent(productMonsterId)
                    .ExcludeMissing();
            
            var inventoryItemIds
                = variants.Select(x => x.ShopifyInventoryItemId).ToList();

            // Retrieve the Inventory Levels and Inventory Items en masse per Product
            var inventoryLevelsJson 
                = _inventoryApi.RetrieveInventoryLevels(inventoryItemIds);

            var inventoryLevels
                = inventoryLevelsJson
                    .DeserializeFromJson<InventoryLevelList>()
                    .inventory_levels;
            
            var inventoryItemsJson
                = _inventoryApi.RetrieveInventoryItems(inventoryItemIds);

            var inventoryItems
                = inventoryItemsJson
                    .DeserializeFromJson<InventoryItemList>()
                    .inventory_items;


            foreach (var variant in variants)
            {
                var variantItem =
                    inventoryItems
                        .First(x => x.id == variant.ShopifyInventoryItemId);

                var variantLevels =
                    inventoryLevels
                        .Where(x => x.inventory_item_id == variant.ShopifyInventoryItemId)
                        .ToList();

                variant.ShopifyCost = variantItem.cost ?? 0m;
                _inventoryRepository.SaveChanges();
                
                UpsertInventory(variant, variantItem, variantLevels);
            }
        }

        public void UpsertInventory(
                    UsrShopifyVariant variant, 
                    InventoryItem shopifyItem,
                    List<InventoryLevel> shopifyLevels)
        {
            var existingLevels =
                _inventoryRepository
                    .RetrieveInventory(variant.ShopifyInventoryItemId);

            var locations = _inventoryRepository.RetreiveLocations();

            foreach (var shopifyLevel in shopifyLevels)
            {
                var existingLevel =
                    existingLevels.FirstOrDefault(x => x.ShopifyLocationId == shopifyLevel.location_id);

                var location 
                    = locations.First(x => x.ShopifyLocationId == shopifyLevel.location_id);

                if (existingLevel == null)
                {
                    var newLevel = new UsrShopifyInventoryLevel();
                    newLevel.ParentMonsterId = variant.MonsterId;
                    newLevel.ShopifyInventoryItemId = shopifyLevel.inventory_item_id;
                    newLevel.ShopifyLocationId = shopifyLevel.location_id;
                    newLevel.ShopifyAvailableQuantity = shopifyLevel.available ?? 0;
                    newLevel.LocationMonsterId = location.MonsterId;
                    newLevel.DateCreated = DateTime.UtcNow;
                    newLevel.LastUpdated = DateTime.UtcNow;

                    _inventoryRepository.InsertInventory(newLevel);
                }
                else
                {
                    existingLevel.ShopifyAvailableQuantity = shopifyLevel.available ?? 0;
                    existingLevel.LastUpdated = DateTime.UtcNow;

                    _inventoryRepository.SaveChanges();
                }
            }
        }


        public void PullDeletedEventsAndUpsert(DateTime filterMinCreated)
        {
            var firstFilter = new EventFilter();
            firstFilter.Page = 1;
            firstFilter.Filter = "product";
            firstFilter.Verb = "destroy";
            firstFilter.CreatedAtMinUtc = filterMinCreated;

            // Pull from Shopify
            var firstJson = _eventApi.Retrieve(firstFilter);
            var firstEvents = firstJson.DeserializeFromJson<EventList>().events;

            UpdateProductsByDeleteEvents(firstEvents);

            var currentPage = 2;

            while (true)
            {
                var currentFilter = firstFilter.Clone();
                currentFilter.Page = currentPage;

                // Pull from Shopify
                var currentJson = _eventApi.Retrieve(currentFilter);
                var currentEvents
                    = currentJson.DeserializeFromJson<EventList>().events;

                UpdateProductsByDeleteEvents(currentEvents);

                if (currentEvents.Count == 0)
                {
                    break;
                }

                currentPage++;
            }
        }

        public void UpdateProductsByDeleteEvents(List<Event> currentEvents)
        {
            foreach (var _event in currentEvents)
            {
                if (_event.verb.ToUpper() != "DESTROY" ||
                    _event.subject_type.ToUpper() != "PRODUCT")
                {
                    continue;
                }

                var product
                    = _inventoryRepository
                        .RetrieveProduct(_event.subject_id);

                product.IsDeleted = true;

                foreach (var variant in product.UsrShopifyVariants)
                {
                    variant.IsMissing = true;
                }

                _inventoryRepository.SaveChanges();
            }
        }

    }
}
