﻿using System;
using Monster.Middle.Persist.Instance;
using Monster.Middle.Processes.Shopify.Persist;
using Monster.Middle.Processes.Sync.Model.Inventory;
using Push.Foundation.Utilities.Json;
using Push.Shopify.Api;
using Push.Shopify.Api.Inventory;

namespace Monster.Middle.Processes.Shopify.Workers
{
    public class ShopifyLocationGet
    {
        private readonly ProductApi _productApi;
        private readonly ShopifyInventoryRepository _locationRepository;
        private readonly ShopifyJsonService _shopifyJsonService;


        public ShopifyLocationGet(
                ProductApi productApi, 
                ShopifyInventoryRepository locationRepository, 
                ShopifyJsonService shopifyJsonService)
        {
            _productApi = productApi;
            _locationRepository = locationRepository;
            _shopifyJsonService = shopifyJsonService;
        }

        public void Run()
        {
            var dataLocations = _locationRepository.RetreiveLocations();

            var shopifyLocations
                    = _productApi
                        .RetrieveLocations()
                        .DeserializeFromJson<LocationList>();

            foreach (var shopifyLoc in shopifyLocations.locations)
            {
                var dataLocation = dataLocations.FindByShopifyId(shopifyLoc);

                using (var transaction = _locationRepository.BeginTransaction())
                {
                    if (dataLocation == null)
                    {
                        var newDataLocation = new ShopifyLocation
                        {
                            ShopifyLocationId = shopifyLoc.id,
                            ShopifyLocationName = shopifyLoc.name,
                            ShopifyActive = shopifyLoc.active,
                            DateCreated = DateTime.UtcNow,
                            LastUpdated = DateTime.UtcNow,
                        };

                        _locationRepository.InsertLocation(newDataLocation);
                    }
                    else
                    {
                        dataLocation.LastUpdated = DateTime.UtcNow;
                        dataLocation.ShopifyLocationName = shopifyLoc.name;
                        dataLocation.ShopifyActive = shopifyLoc.active;
                    }
                    _locationRepository.SaveChanges();
                    _shopifyJsonService.Upsert(ShopifyJsonType.Location, shopifyLoc.id, shopifyLoc.SerializeToJson());

                    transaction.Commit();
                }
            }
        }

    }
}
