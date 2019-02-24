using System.Linq;
using Monster.Middle.Persist.Multitenant;
using Monster.Middle.Processes.Sync.Extensions;

namespace Monster.Middle.Processes.Sync.Inventory.Workers
{
    public class WarehouseLocationSync
    {
        private readonly SyncInventoryRepository _repository;
        private readonly ExecutionLogRepository _executionLogRepository;

        public WarehouseLocationSync(
                SyncInventoryRepository repository, 
                ExecutionLogRepository executionLogRepository)
        {
            _repository = repository;
            _executionLogRepository = executionLogRepository;
        }

        // Disconnects Warehouses from Deactivated Locations
        //
        public void Run()
        {
            var locations = _repository.RetrieveDeactivatedMatchedLocations();
            
            foreach (var location in locations)
            {
                // The ToList() allows us to mutate the collection
                foreach (var sync in location.UsrShopAcuWarehouseSyncs.ToList())
                {
                    var log = $"Removed match from Deactivated Location {location.ShopifyLocationName} " +
                        $"to Warehouse {sync.UsrAcumaticaWarehouse.AcumaticaWarehouseId}";

                    _repository.DeleteWarehouseSync(sync);
                    _executionLogRepository.InsertExecutionLog(log);
                }
            }
        }

    }
}
