using System;
using System.Linq;
using Monster.Middle.Processes.Sync.Extensions;
using Monster.Middle.Processes.Sync.Persist;
using Push.Foundation.Utilities.Logging;
using Push.Shopify.Api;

namespace Monster.Middle.Processes.Sync.Inventory.Workers
{
    public class ShopifyLocationSync
    {
        private readonly SyncInventoryRepository _repository;
        private readonly InventoryApi  _inventoryApi;
        private readonly IPushLogger _logger;

        public ShopifyLocationSync(
                IPushLogger logger,
                InventoryApi inventoryApi,
                SyncInventoryRepository repository)
        {
            _logger = logger;
            _inventoryApi = inventoryApi;
            _repository = repository;
        }


        // TODO - add a Preference for whether or not to push
        // ... Shopify Locations into Acumatica Warehouse

        public void Run()
        {
            var shopifyLocations = _repository.RetrieveLocations();
            var warehouses = _repository.RetrieveWarehouses();

            foreach (var warehouse in warehouses)
            {
                if (warehouse.HasMatch())
                {
                    var sync = warehouse.UsrShopAcuWarehouseSyncs.First();
                    var location = sync.UsrShopifyLocation;

                    if (location.IsImproperlyMatched())
                    {
                        _repository.DeleteWarehouseSync(sync);
                    }

                    // TODO - revisit
                    //sync.IsNameMismatched = !location.MatchesIdWithName(warehouse);
                    //sync.LastUpdated = DateTime.UtcNow;
                    //_repository.SaveChanges();                    

                    continue;
                }

                // Attempt to automatically match
                var automatchLocation = 
                    shopifyLocations.FirstOrDefault(x => warehouse.AutoMatches(x));

                if (automatchLocation != null)
                {
                    _repository.InsertWarehouseSync(automatchLocation, warehouse);
                    continue;
                }
                
                // *** NOTE - Shopify does not allow for writing Locations
            }
        }
    }
}
