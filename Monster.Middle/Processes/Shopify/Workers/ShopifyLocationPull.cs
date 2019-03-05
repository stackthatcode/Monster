using System;
using Monster.Middle.Persist.Tenant;
using Monster.Middle.Processes.Shopify.Persist;
using Monster.Middle.Processes.Sync.Model.Extensions;
using Push.Foundation.Utilities.Json;
using Push.Foundation.Utilities.Logging;
using Push.Shopify.Api;
using Push.Shopify.Api.Inventory;

namespace Monster.Middle.Processes.Shopify.Workers
{
    public class ShopifyLocationPull
    {
        private readonly ProductApi _productApi;
        private readonly ShopifyInventoryRepository _locationRepository;
        private readonly ShopifyBatchRepository _shopifyBatchRepository;
        private readonly IPushLogger _logger;

        public ShopifyLocationPull(
                ProductApi productApi,
                ShopifyInventoryRepository locationRepository, 
                ShopifyBatchRepository shopifyBatchRepository, 
                IPushLogger logger)
        {
            _productApi = productApi;
            _locationRepository = locationRepository;
            _shopifyBatchRepository = shopifyBatchRepository;
            _logger = logger;
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

                if (dataLocation == null)
                {
                    var newDataLocation = new UsrShopifyLocation
                    {
                        ShopifyLocationId = shopifyLoc.id,
                        ShopifyJson = shopifyLoc.SerializeToJson(),
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
                    dataLocation.ShopifyJson = shopifyLoc.SerializeToJson();
                    dataLocation.ShopifyLocationName = shopifyLoc.name;
                    dataLocation.ShopifyActive = shopifyLoc.active;

                    _locationRepository.SaveChanges();
                }
            }
        }

    }
}
