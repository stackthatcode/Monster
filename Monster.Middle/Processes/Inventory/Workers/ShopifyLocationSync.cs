using System;
using System.Linq;
using Monster.Middle.Persist.Multitenant;
using Monster.Middle.Persist.Multitenant.Extensions;
using Push.Foundation.Utilities.Logging;
using Push.Shopify.Api;


namespace Monster.Middle.Processes.Inventory.Workers
{
    public class ShopifyLocationSync
    {
        private readonly LocationRepository _locationRepository;
        private readonly InventoryApi  _inventoryApi;
        private readonly IPushLogger _logger;

        public ShopifyLocationSync(
                    LocationRepository locationRepository,
                    InventoryApi inventoryApi,
                    IPushLogger logger)
        {
            _locationRepository = locationRepository;
            _inventoryApi = inventoryApi;
            _logger = logger;
        }

        public void Run()
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
                var automatch = 
                    shopifyLocations.FirstOrDefault(x => warehouse.AutoMatches(x));

                if (automatch != null)
                {
                    warehouse.ShopifyLocationMonsterId = automatch.MonsterId;
                    warehouse.LastUpdated = DateTime.UtcNow;
                    _locationRepository.SaveChanges();
                    continue;
                }
                
                // *** NOTE - Shopify does not allow for writing Locations
            }
        }
    }
}
