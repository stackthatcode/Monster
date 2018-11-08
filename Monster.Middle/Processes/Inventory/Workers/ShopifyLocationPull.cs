using System;
using Monster.Middle.Persist.Multitenant;
using Monster.Middle.Persist.Multitenant.Etc;
using Monster.Middle.Persist.Multitenant.Extensions;
using Monster.Middle.Persist.Multitenant.Shopify;
using Push.Foundation.Utilities.Json;
using Push.Foundation.Utilities.Logging;
using Push.Shopify.Api;
using Push.Shopify.Api.Inventory;


namespace Monster.Middle.Processes.Inventory.Workers
{
    public class ShopifyLocationPull
    {
        private readonly ProductApi _productApi;
        private readonly ShopifyInventoryRepository _locationRepository;
        private readonly BatchStateRepository _batchStateRepository;
        private readonly IPushLogger _logger;

        public ShopifyLocationPull(
                ProductApi productApi,
                ShopifyInventoryRepository locationRepository, 
                BatchStateRepository batchStateRepository, 
                IPushLogger logger)
        {
            _productApi = productApi;
            _locationRepository = locationRepository;
            _batchStateRepository = batchStateRepository;
            _logger = logger;
        }

        public void Run()
        {
            var dataLocations 
                    = _locationRepository.RetreiveLocations();

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

                    _locationRepository.InsertLocation(newDataLocation);
                }
                else
                {
                    dataLocation.LastUpdated = DateTime.UtcNow;
                    dataLocation.ShopifyJson = shopifyLoc.SerializeToJson();
                    dataLocation.ShopifyLocationName = shopifyLoc.name;

                    _locationRepository.SaveChanges();
                }
            }
        }

    }
}
