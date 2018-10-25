using System;
using System.Collections.Generic;
using System.Linq;
using Monster.Acumatica.Api;
using Monster.Acumatica.Api.Common;
using Monster.Acumatica.Api.Distribution;
using Monster.Middle.Persist.Multitenant;
using Monster.Middle.Persist.Multitenant.Extensions;
using Push.Foundation.Utilities.Json;
using Push.Foundation.Utilities.Logging;
using Push.Shopify.Api;


namespace Monster.Middle.Processes.Inventory.Workers
{
    public class ShopifyWarehouseSync
    {
        private readonly LocationRepository _locationRepository;
        private readonly InventoryApi  _inventoryApi;
        private readonly IPushLogger _logger;

        public ShopifyWarehouseSync(
                    LocationRepository locationRepository,
                    InventoryApi inventoryApi,
                    IPushLogger logger)
        {
            _locationRepository = locationRepository;
            _inventoryApi = inventoryApi;
            _logger = logger;
        }

        public void Synchronize()
        {
            var shopifyLocations 
                    = _locationRepository.RetreiveShopifyLocations();

            var warehouses = _locationRepository.RetreiveAcumaticaWarehouses();

            // TODO - add a Preference for whether or not to push
            // ... Shopify Locations into Acumatica Warehouse

            foreach (var warehouse in warehouses)
            {
                if (warehouse.UsrShopifyLocation != null)
                {
                    var matchedLocation = warehouse.UsrShopifyLocation;

                    // Flag if the names mismatch, now
                    warehouse.IsNameMismatched =
                        !matchedLocation.MatchesIdWithName(warehouse);

                    warehouse.LastUpdated = DateTime.UtcNow;
                    _locationRepository.SaveChanges();                    
                    continue;
                }

                // Attempt to automatically match
                var automatch = shopifyLocations
                        .FirstOrDefault(x => warehouse.AutoMatches(x));

                if (automatch != null)
                {
                    warehouse.ShopifyLocationMonsterId = automatch.MonsterId;
                    warehouse.LastUpdated = DateTime.UtcNow;
                    continue;
                }
                
                // *** NOTE - Shopify does not allow for writing Locations
            }
        }
    }
}
