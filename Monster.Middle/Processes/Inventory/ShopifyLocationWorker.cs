using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Monster.Middle.Persist.Multitenant;
using Monster.Middle.Persist.Multitenant.Extensions;
using Push.Foundation.Utilities.Json;
using Push.Foundation.Utilities.Logging;
using Push.Shopify.Api;
using Push.Shopify.Api.Inventory;

namespace Monster.Middle.Processes.Inventory
{
    public class ShopifyLocationWorker
    {
        private readonly ProductApi _productApi;
        private readonly LocationRepository _locationRepository;
        private readonly BatchStateRepository _batchStateRepository;
        private readonly IPushLogger _logger;

        public ShopifyLocationWorker(
                ProductApi productApi, 
                LocationRepository locationRepository, 
                BatchStateRepository batchStateRepository, 
                IPushLogger logger)
        {
            _productApi = productApi;
            _locationRepository = locationRepository;
            _batchStateRepository = batchStateRepository;
            _logger = logger;
        }

        public void BaselinePull()
        {
            var dataLocations = _locationRepository.RetreiveShopifyLocations();

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

                    _locationRepository.InsertShopifyLocation(newDataLocation);
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
