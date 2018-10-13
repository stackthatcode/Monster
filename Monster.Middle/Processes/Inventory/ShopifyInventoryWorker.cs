﻿using System;
using Monster.Middle.Persist.Multitenant;
using Monster.Middle.Persist.Multitenant.Extensions;
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
        private readonly IPushLogger _logger;

        public ShopifyInventoryWorker(
                ProductApi productApi, 
                InventoryRepository inventoryRepository, 
                IPushLogger logger)
        {
            _productApi = productApi;
            _inventoryRepository = inventoryRepository;
            _logger = logger;
        }


        public void PullLocationsFromShopify()
        {
            var dataLocations = _inventoryRepository.RetreiveShopifyLocations();

            var shopifyLocations
                = _productApi
                    .RetrieveLocations()
                    .DeserializeFromJson<LocationList>();

            foreach (var shopifyLoc in shopifyLocations.locations)
            {
                var dataLocation = dataLocations.FindByShopifyId(shopifyLoc);

                if (dataLocation == null)
                {
                    var newDataLocation = new UsrShopifyLocation
                    {
                        ShopifyLocationId = shopifyLoc.id,
                        ShopifyJson = shopifyLoc.SerializeToJson(),
                        ShopifyLocationName = shopifyLoc.name,
                        DateCreated = DateTime.UtcNow,
                        LastUpdated = DateTime.UtcNow,
                    };

                    _inventoryRepository.InsertShopifyLocation(newDataLocation);
                }
                else
                {
                    dataLocation.LastUpdated = DateTime.UtcNow;
                    dataLocation.ShopifyJson = shopifyLoc.SerializeToJson();
                    dataLocation.ShopifyLocationName = shopifyLoc.name;

                    _inventoryRepository.SaveChanges();
                }
            }
        }


        // TODO - add Debug logging
        //
        public void PullProducts(ProductFilter filter)
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
                currentFilter.Page = currentPage;
                
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

                parentId = data.Id;
            }
            else
            {
                existing.ShopifyJson = product.SerializeToJson();
                existing.LastUpdated = DateTime.UtcNow;
                _inventoryRepository.SaveChanges();

                parentId = existing.Id;
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

                _inventoryRepository.SaveChanges();
            }
        }

        public void PushFromShopifyToAcumatica()
        {
            // For each Product

        }

    }
}