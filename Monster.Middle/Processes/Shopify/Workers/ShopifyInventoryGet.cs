using System;
using System.Collections.Generic;
using System.Linq;
using Monster.Middle.Misc.Hangfire;
using Monster.Middle.Misc.Logging;
using Monster.Middle.Persist.Instance;
using Monster.Middle.Processes.Shopify.Persist;
using Monster.Middle.Processes.Sync.Misc;
using Push.Foundation.Utilities.General;
using Push.Foundation.Utilities.Json;
using Push.Foundation.Utilities.Logging;
using Push.Shopify.Api;
using Push.Shopify.Api.Event;
using Push.Shopify.Api.Inventory;
using Push.Shopify.Api.Product;

namespace Monster.Middle.Processes.Shopify.Workers
{
    public class ShopifyInventoryGet
    {
        private readonly ProductApi _productApi;
        private readonly InventoryApi _inventoryApi;
        private readonly EventApi _eventApi;
        private readonly ShopifyInventoryRepository _inventoryRepository;
        private readonly JobMonitoringService _jobMonitoringService;
        private readonly ShopifyBatchRepository _batchRepository;
        private readonly ExecutionLogService _executionLogService;
        private readonly ShopifyJsonService _shopifyJsonService;
        private readonly IPushLogger _logger;
        
        public ShopifyInventoryGet(
                IPushLogger logger,
                ProductApi productApi,
                InventoryApi inventoryApi,
                EventApi eventApi,
                ShopifyInventoryRepository inventoryRepository,
                JobMonitoringService jobMonitoringService,
                ShopifyBatchRepository batchRepository, 
                ExecutionLogService executionLogService, ShopifyJsonService shopifyJsonService)
        {
            _productApi = productApi;
            _inventoryApi = inventoryApi;
            _eventApi = eventApi;
            _inventoryRepository = inventoryRepository;
            _jobMonitoringService = jobMonitoringService;
            _batchRepository = batchRepository;
            _executionLogService = executionLogService;
            _shopifyJsonService = shopifyJsonService;
            _logger = logger;
        }

        public void RunAutomatic()
        {
            var batchState = _batchRepository.Retrieve();
            _executionLogService.Log("Refreshing Inventory from Shopify");

            if (batchState.ShopifyProductsGetEnd.HasValue)
            {
                var firstFilter = new SearchFilter();
                firstFilter.UpdatedAtMinUtc = batchState.ShopifyProductsGetEnd.Value;
                firstFilter.Page = 1;

                Run(firstFilter);
            }
            else
            {
                // For the first run, we pull the complete catalog of Shopify Products
                var firstFilter = new SearchFilter();
                firstFilter.Page = 1;

                Run(firstFilter);
            }
        }

        private void Run(SearchFilter firstFilter)
        {
            if (_jobMonitoringService.DetectCurrentJobInterrupt())
            {
                return;
            }

            // We've hanging on to this to compute the end of the Batch State
            var startOfRun = DateTime.UtcNow;
            var firstJson = _productApi.RetrieveProduct(firstFilter);
            var firstProducts = firstJson.DeserializeFromJson<ProductList>().products;

            UpsertProductsAndInventory(firstProducts);

            var currentPage = 2;

            while (true)
            {
                if (_jobMonitoringService.DetectCurrentJobInterrupt())
                {
                    return;
                }

                var currentFilter = firstFilter.Clone();
                currentFilter.Page = currentPage;
                
                var currentJson = _productApi.RetrieveProduct(currentFilter);
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
            var batchEnd = startOfRun.SubtractFudgeFactor();
            _batchRepository.UpdateProductsGetEnd(batchEnd);            
        }

        public long Run(long shopifyProductId)
        {
            var productJson = _productApi.RetrieveProduct(shopifyProductId);
            var product = productJson.DeserializeFromJson<ProductParent>();
            return UpsertProductAndInventory(product.product);
        }

        public void UpsertProductsAndInventory(IEnumerable<Product> products)
        {
            foreach (var product in products)
            {
                if (_jobMonitoringService.DetectCurrentJobInterrupt())
                {
                    return;
                }

                UpsertProductAndInventory(product);
            }
        }

        public long UpsertProductAndInventory(Product product)
        {
            long productMonsterId;

            using (var transaction = _inventoryRepository.BeginTransaction())
            {
                var existing = _inventoryRepository.RetrieveProduct(product.id);
                if (existing == null)
                {
                    var data = new ShopifyProduct();
                    data.ShopifyProductId = product.id;
                    data.ShopifyTitle = product.title ?? "";
                    data.ShopifyProductType = product.product_type;
                    data.ShopifyVendor = product.vendor;
                    data.DateCreated = DateTime.UtcNow;
                    data.LastUpdated = DateTime.UtcNow;

                    _inventoryRepository.InsertProduct(data);
                    _inventoryRepository.SaveChanges();

                    productMonsterId = data.MonsterId;
                }
                else
                {
                    existing.ShopifyTitle = product.title ?? "";
                    existing.ShopifyProductType = product.product_type;
                    existing.ShopifyVendor = product.vendor;
                    existing.LastUpdated = DateTime.UtcNow;
                    _inventoryRepository.SaveChanges();

                    productMonsterId = existing.MonsterId;
                }

                _shopifyJsonService.Upsert(ShopifyJsonType.Product, product.id, product.SerializeToJson());
                transaction.Commit();
            }


            // Write the Product Variants
            UpsertProduct(productMonsterId, product);

            // Flags the missing Variants
            ProcessMissingVariants(productMonsterId, product);

            // Pull and write the Inventory
            PullAndUpsertInventory(productMonsterId);

            return productMonsterId;
        }

        public void UpsertProduct(long parentMonsterId, Product product)
        {
            foreach (var variant in product.variants)
            {
                UpsertVariant(parentMonsterId, variant);
            }
        }

        public void UpsertVariant(long parentProductId, Variant variant)
        {
            var existing = _inventoryRepository.RetrieveVariant(variant.id);
                
            if (existing == null)
            {
                CreateNewVariantRecord(parentProductId, variant);
            }
            else
            {


                using (var transaction = _inventoryRepository.BeginTransaction())
                {
                    existing.ShopifySku = variant.sku;
                    existing.ShopifyTitle = variant.title ?? "";
                    existing.ShopifyIsTaxable = variant.taxable;
                    existing.ShopifyPrice = (decimal) variant.price;
                    existing.LastUpdated = DateTime.UtcNow;

                    _shopifyJsonService.Upsert(ShopifyJsonType.Variant, variant.id, variant.SerializeToJson());
                    _inventoryRepository.SaveChanges();
                    transaction.Commit();
                }
            }
        }

        public ShopifyVariant CreateNewVariantRecord(long parentProductId, Variant variant)
        {
            using (var transaction = _inventoryRepository.BeginTransaction())
            {
                var data = new ShopifyVariant();
                data.ParentMonsterId = parentProductId;
                data.ShopifyVariantId = variant.id;
                data.ShopifySku = variant.sku;
                data.ShopifyTitle = variant.title ?? "";
                data.ShopifyInventoryItemId = variant.inventory_item_id;
                data.ShopifyIsTaxable = variant.taxable;
                data.ShopifyPrice = (decimal) variant.price;
                data.IsMissing = false;
                data.DateCreated = DateTime.UtcNow;
                data.LastUpdated = DateTime.UtcNow;

                _inventoryRepository.InsertVariant(data);
                _shopifyJsonService.Upsert(ShopifyJsonType.Variant, variant.id, variant.SerializeToJson());
                transaction.Commit();

                return data;
            }
        }

        public void ProcessMissingVariants(long parentMonsterId, Product shopifyProduct)
        {
            var storedVariants = _inventoryRepository.RetrieveVariantsByParent(parentMonsterId);

            foreach (var storedVariant in storedVariants)
            {
                if (shopifyProduct.variants.All(x => x.id != storedVariant.ShopifyVariantId))
                {
                    ProcessMissingVariant(storedVariant);
                }
            }
        }

        public void ProcessMissingVariant(ShopifyVariant variantRecord)
        {
            var log = $"Shopify Variant {variantRecord.ShopifySku} ({variantRecord.ShopifyVariantId}) is missing";
            _logger.Debug(log);

            using (var transaction = _inventoryRepository.BeginTransaction())
            {
                // Flag as Missing and destroy synchronization
                //
                variantRecord.IsMissing = true;
                var stockItemRecord = variantRecord.AcumaticaStockItems.FirstOrDefault();

                if (stockItemRecord != null)
                {
                    // Remove the synchronization
                    //
                    stockItemRecord.ShopifyVariant = null;

                    // Locate replacement Variant to sync with
                    //
                    var replacements =
                        variantRecord
                            .ShopifyProduct
                            .NonMissingVariants()
                            .Where(x => x.ShopifySku.StandardizedSku() == variantRecord.ShopifySku.StandardizedSku())
                            .ToList();

                    // Either no viable Duplicates, abort
                    //
                    if (replacements.Count == 1)
                    {
                        stockItemRecord.ShopifyVariant = replacements.First();
                    }

                    stockItemRecord.LastUpdated = DateTime.UtcNow;
                    _inventoryRepository.SaveChanges();
                }

                transaction.Commit();
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
                var inventoryItem = inventoryItems.First(x => x.id == variant.ShopifyInventoryItemId);

                var inventoryLevel =
                    inventoryLevels
                        .Where(x => x.inventory_item_id == variant.ShopifyInventoryItemId)
                        .ToList();

                variant.ShopifyCost = inventoryItem.cost ?? 0m;
                variant.ShopifyIsTracked = inventoryItem.tracked == true;
                _inventoryRepository.SaveChanges();
                
                UpsertInventory(variant, inventoryItem, inventoryLevel);
            }
        }

        public void UpsertInventory(
                    ShopifyVariant variant, 
                    InventoryItem shopifyItem,
                    List<InventoryLevel> shopifyLevels)
        {
            var existingLevels =
                _inventoryRepository.RetrieveInventory(variant.ShopifyInventoryItemId);

            var locations = _inventoryRepository.RetreiveLocations();

            foreach (var shopifyLevel in shopifyLevels)
            {
                var existingLevel =
                    existingLevels.FirstOrDefault(x => x.ShopifyLocationId == shopifyLevel.location_id);

                var location 
                    = locations.First(x => x.ShopifyLocationId == shopifyLevel.location_id);

                if (existingLevel == null)
                {
                    var newLevel = new ShopifyInventoryLevel();
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

                var product = _inventoryRepository.RetrieveProduct(_event.subject_id);
                if (product == null)
                {
                    continue;
                }

                product.IsDeleted = true;

                foreach (var variant in product.ShopifyVariants)
                {
                    // Flag Variant as missing and break synchronization with Stock Item
                    //
                    variant.IsMissing = true;
                    variant.AcumaticaStockItems.ForEach(x => x.ShopifyVariant = null);
                }

                _inventoryRepository.SaveChanges();
            }
        }
    }
}

