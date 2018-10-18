using System;
using System.Collections.Generic;
using Monster.Middle.Persist.Multitenant;
using Monster.Middle.Persist.Multitenant.Extensions;
using Push.Foundation.Utilities.General;
using Push.Foundation.Utilities.Json;
using Push.Foundation.Utilities.Logging;
using Push.Shopify.Api;
using Push.Shopify.Api.Product;
using Push.Shopify.Api.Inventory;

namespace Monster.Middle.Processes.Inventory
{
    public class ShopifyInventoryWorker
    {
        private readonly ProductApi _productApi;
        private readonly InventoryRepository _inventoryRepository;
        private readonly BatchStateRepository _batchStateRepository;
        private readonly IPushLogger _logger;

        public const int InitialBatchStateFudgeMin = -15;


        public ShopifyInventoryWorker(
                ProductApi productApi, 
                InventoryRepository inventoryRepository, 
                BatchStateRepository batchStateRepository,
                IPushLogger logger)
        {
            _productApi = productApi;
            _inventoryRepository = inventoryRepository;
            _batchStateRepository = batchStateRepository;
            _logger = logger;
        }

        public void BaselinePullProducts()
        {
            _logger.Debug("Baseline Pull Products");

            var firstFilter = new ProductFilter();
            firstFilter.Page = 1;

            var firstJson = _productApi.Retrieve(firstFilter);
            var firstProducts = firstJson.DeserializeFromJson<ProductList>().products;
            UpsertProducts(firstProducts);

            var currentPage = 2;

            while (true)
            {
                var currentFilter = firstFilter.Clone();
                currentFilter.Page = currentPage;
                
                var currentJson = _productApi.Retrieve(currentFilter);
                var currentProducts = currentJson.DeserializeFromJson<ProductList>().products;
                UpsertProducts(currentProducts);

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

        public void DiffSyncPullProducts()
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
            UpsertProducts(firstProducts);

            var currentPage = 2;

            while (true)
            {
                var currentFilter = firstFilter.Clone();
                currentFilter.Page = currentPage;

                // Pull from Shopify
                var currentJson = _productApi.Retrieve(currentFilter);
                var currentProducts = currentJson.DeserializeFromJson<ProductList>().products;
                UpsertProducts(currentProducts);

                if (currentProducts.Count == 0)
                {
                    break;
                }

                currentPage++;
            }
            
            _batchStateRepository.UpdateShopifyProductsPullEnd(startOfPullRun);
        }


        public void UpsertProducts(IEnumerable<Product> products)
        {
            products.ForEach(x => UpsertProduct(x));
        }

        public void UpsertProduct(Product product)
        {
            var existing =
                _inventoryRepository.RetrieveShopifyProduct(product.id);

            var parentId = (long?) null;

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

                parentId = data.MonsterId;
            }
            else
            {
                existing.ShopifyJson = product.SerializeToJson();
                existing.LastUpdated = DateTime.UtcNow;
                _inventoryRepository.SaveChanges();

                parentId = existing.MonsterId;
            }

            foreach (var variant in product.variants)
            {
                UpsertVariant(parentId.Value, variant);
            }
        }

        public void UpsertVariant(long parentProductId, Variant variant)
        {
            var existing = 
                _inventoryRepository
                    .RetrieveShopifyVariants(variant.id, variant.sku);
            
            if (existing == null)
            {
                var data = new UsrShopifyVariant
                {
                    ParentMonsterId = parentProductId,
                    ShopifyVariantId = variant.id,
                    ShopifySku = variant.sku,
                    ShopifyJson = variant.SerializeToJson(),
                    DateCreated = DateTime.UtcNow,
                    LastUpdated = DateTime.UtcNow,
                };
                
                _inventoryRepository.InsertShopifyVariant(data);
            }
            else
            {
                existing.ShopifyJson = variant.SerializeToJson();
                existing.LastUpdated = DateTime.UtcNow;

                _inventoryRepository.SaveChanges();
            }
        }

        public void PushFromShopifyToAcumatica()
        {
            // For each Product

        }

    }
}
