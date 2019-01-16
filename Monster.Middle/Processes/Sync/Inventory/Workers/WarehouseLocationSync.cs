using System.Linq;
using Monster.Middle.Persist.Multitenant;
using Monster.Middle.Processes.Sync.Extensions;

namespace Monster.Middle.Processes.Sync.Inventory.Workers
{
    public class WarehouseLocationSync
    {
        private readonly SyncInventoryRepository _repository;
        private readonly StateRepository _stateRepository;

        public WarehouseLocationSync(SyncInventoryRepository repository)
        {
            _repository = repository;
        }

        public void Run()
        {
            RunLocationMatch();
            RunWarehouseMatch();
        }


        private void RunLocationMatch()
        {
            var shopifyLocations = _repository.RetrieveLocations();
            var warehouses = _repository.RetrieveWarehouses();
            
            foreach (var shopifyLocation in shopifyLocations)
            {
                if (shopifyLocation.UsrShopAcuWarehouseSyncs.Any())
                {                    
                    if (shopifyLocation.IsImproperlyMatched())
                    {
                        _repository.DeleteWarehouseSync(
                                shopifyLocation
                                    .UsrShopAcuWarehouseSyncs.First());
                    }

                    continue;
                }

                // Attempt to automatically match
                var automatchedWarehouse = warehouses
                        .FirstOrDefault(x => x.AutoMatches(shopifyLocation));

                if (automatchedWarehouse != null)
                {
                    _repository
                        .InsertWarehouseSync(shopifyLocation, automatchedWarehouse);
                    continue;
                }
            }
        }

        private void RunWarehouseMatch()
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
            }
        }

    }
}
