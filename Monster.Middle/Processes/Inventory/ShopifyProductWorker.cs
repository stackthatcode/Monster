using System;
using Monster.Middle.Persist.Multitenant;
using Push.Foundation.Utilities.Json;
using Push.Foundation.Utilities.Logging;
using Push.Shopify.Api;
using Push.Shopify.Api.Product;

namespace Monster.Middle.Processes.Inventory
{
    public class ShopifyProductWorker
    {
        private readonly ProductApi _productApi;
        private readonly InventoryRepository _inventoryRepository;
        private readonly IPushLogger _logger;

        public ShopifyProductWorker(
                ProductApi productApi, 
                InventoryRepository inventoryRepository, 
                IPushLogger logger)
        {
            _productApi = productApi;
            _inventoryRepository = inventoryRepository;
            _logger = logger;
        }


        // TODO - add Debug logging
        //
        public void PullFromShopify(ProductFilter filter)
        {
            var firstFilter = filter.Clone();
            firstFilter.Page = 1;

            var firstJson = _productApi.Retrieve(firstFilter);
            var firstProducts = firstJson
                    .DeserializeFromJson<ProductList>().products;
            firstProducts.ForEach(UpsertProduct);

            var currentPage = 2;

            while (true)
            {
                var currentFilter = filter.Clone();
                firstFilter.Page = currentPage;
                
                var currentJson = _productApi.Retrieve(currentFilter);
                var currentProducts = currentJson
                        .DeserializeFromJson<ProductList>().products;
                currentProducts.ForEach(UpsertProduct);

                if (currentProducts.Count == 0)
                {
                    break;
                }

                currentPage++;
            }
        }

        public void UpsertProduct(Product product)
        {
            var existing =
                _inventoryRepository.RetrieveShopifyProduct(product.id);

            var shopifyProductId = (long?) null;

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

                shopifyProductId = data.ShopifyProductId;
            }
            else
            {
                existing.ShopifyJson = product.SerializeToJson();
                existing.LastUpdated = DateTime.UtcNow;

                shopifyProductId = existing.ShopifyProductId;
            }

            foreach (var variant in product.variants)
            {
                UpsertVariant(shopifyProductId.Value, variant);
            }
        }

        public void UpsertVariant(long parentProductId, Variant variant)
        {
            var existing = 
                _inventoryRepository
                    .RetrieveShopifyVariants(variant.id, variant.sku);
            
            if (existing != null)
            {
                var data = new UsrShopifyVariant
                {
                    ParentProductId = parentProductId,
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
            }
        }

        public void PushFromShopifyToAcumatica()
        {
            // For each Product

        }

    }
}
