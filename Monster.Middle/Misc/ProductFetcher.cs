using System;
using Monster.Middle.Persist.Multitenant;
using Push.Foundation.Utilities.General;
using Push.Foundation.Utilities.Json;
using Push.Foundation.Utilities.Logging;
using Push.Shopify.Api;
using Push.Foundation.Web.Helpers;
using Push.Shopify.Api.Product;

namespace Monster.Middle.Misc
{
    public class ProductFetcher
    {
        private readonly ProductApi _productApi;
        private readonly InventoryRepository _inventoryRepository;
        private readonly IPushLogger _logger;

        public ProductFetcher(
                ProductApi productApi, 
                InventoryRepository inventoryRepository, 
                IPushLogger logger)
        {
            _productApi = productApi;
            _inventoryRepository = inventoryRepository;
            _logger = logger;
        }




        public void FetchAllProducts(ProductFilter filter)
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
            }
            else
            {
                existing.ShopifyJson = product.SerializeToJson();
                existing.LastUpdated = DateTime.UtcNow;
            }
        }
    }
}
