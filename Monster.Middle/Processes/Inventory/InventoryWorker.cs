using System;
using System.Linq;
using Monster.Acumatica.Api;
using Monster.Middle.Persist.Multitenant;
using Monster.Middle.Persist.Multitenant.Extensions;
using Push.Foundation.Utilities.Json;
using Push.Foundation.Utilities.Logging;
using Push.Shopify.Api;
using Push.Shopify.Api.Inventory;
using InventoryRepository = Monster.Middle.Persist.Multitenant.InventoryRepository;

namespace Monster.Middle.Processes.Inventory
{
    public class InventoryWorker
    {
        private readonly InventoryRepository _inventoryRepository;
        private readonly InventoryApi _shopifyInventoryApi;
        private readonly SessionRepository _sessionRepository;
        private readonly Acumatica.Api.InventoryRepository _acuInventoryRepository;
        private readonly IPushLogger _logger;

        public InventoryWorker(
                IPushLogger logger,
                InventoryRepository inventoryRepository, 
                InventoryApi shopifyInventoryApi, 
                Acumatica.Api.SessionRepository sessionRepository,
                Acumatica.Api.InventoryRepository acuInventoryRepository)
        {
            _acuInventoryRepository = acuInventoryRepository;
            _inventoryRepository = inventoryRepository;
            _shopifyInventoryApi = shopifyInventoryApi;
            _sessionRepository = sessionRepository;
            _logger = logger;
        }

        public void PullLocationsFromShopify()
        {
            var dataLocations = _inventoryRepository.RetreiveLocations();

            var shopifyLocations
                = _shopifyInventoryApi
                        .RetrieveLocations()
                        .DeserializeFromJson<LocationList>();

            foreach (var shopifyLoc in shopifyLocations.locations)
            {
                var dataLocation = dataLocations.Find(shopifyLoc);

                if (dataLocation == null)
                {
                    var newDataLocation = new UsrShopifyLocation
                    {
                        ShopifyLocationId = shopifyLoc.id,
                        ShopifyJson = shopifyLoc.SerializeToJson(),
                        DateCreated = DateTime.UtcNow,
                        LastUpdated = DateTime.UtcNow,
                    };
                    
                    _inventoryRepository.InsertLocation(newDataLocation);
                }
                else
                {
                    dataLocation.LastUpdated = DateTime.UtcNow;
                    dataLocation.ShopifyJson = shopifyLoc.SerializeToJson();
                }
            }

            // TODO - flag Locations missing from Shopify

        }

        public void PullWarehousesFromAcumatica()
        {
            var locations = _acuInventoryRepository.RetrieveWarehouses();
        }
    }
}
