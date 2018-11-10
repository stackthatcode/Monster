using System;
using System.Linq;
using Monster.Middle.Persist.Multitenant;
using Monster.Middle.Persist.Multitenant.Acumatica;
using Monster.Middle.Persist.Multitenant.Extensions;
using Monster.Middle.Persist.Multitenant.Shopify;
using Monster.Middle.Persist.Multitenant.Sync;
using Push.Foundation.Utilities.Logging;
using Push.Shopify.Api;


namespace Monster.Middle.Processes.Inventory.Workers
{
    public class ShopifyLocationSync
    {
        private readonly ShopifyInventoryRepository _shopifyInventoryRepository;
        private readonly AcumaticaInventoryRepository _acumaticaInventoryRepository;
        private readonly InventoryApi  _inventoryApi;
        private readonly IPushLogger _logger;

        public ShopifyLocationSync(
                    ShopifyInventoryRepository shopifyInventoryRepository,
                    AcumaticaInventoryRepository acumaticaInventoryRepository,
                    InventoryApi inventoryApi,
                    IPushLogger logger)
        {
            _shopifyInventoryRepository = shopifyInventoryRepository;
            _acumaticaInventoryRepository = acumaticaInventoryRepository;
            _inventoryApi = inventoryApi;
            _logger = logger;
        }

        public void Run()
        {
            var shopifyLocations 
                    = _shopifyInventoryRepository.RetreiveLocations();

            var warehouses = _acumaticaInventoryRepository.RetreiveWarehouses();

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
                    _shopifyInventoryRepository.SaveChanges();                    
                    continue;
                }

                // Attempt to automatically match
                var automatch = 
                    shopifyLocations.FirstOrDefault(x => warehouse.AutoMatches(x));

                if (automatch != null)
                {
                    warehouse.ShopifyLocationMonsterId = automatch.MonsterId;
                    warehouse.LastUpdated = DateTime.UtcNow;
                    _shopifyInventoryRepository.SaveChanges();
                    continue;
                }
                
                // *** NOTE - Shopify does not allow for writing Locations
            }
        }
    }
}
