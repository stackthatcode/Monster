using System.Linq;
using Monster.Middle.Processes.Sync.Persist;
using Monster.Middle.Processes.Sync.Services;

namespace Monster.Middle.Processes.Sync.Workers.Inventory
{
    public class WarehouseLocationSync
    {
        private readonly SyncInventoryRepository _repository;
        private readonly ExecutionLogService _executionLogService;

        public WarehouseLocationSync(
                SyncInventoryRepository repository, 
                ExecutionLogService executionLogService)
        {
            _repository = repository;
            _executionLogService = executionLogService;
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
                    using (var transaction = _repository.BeginTransaction())
                    {
                        var log = $"Removed match from Deactivated Location {location.ShopifyLocationName} " +
                                  $"to Warehouse {sync.UsrAcumaticaWarehouse.AcumaticaWarehouseId}";

                        _repository.DeleteWarehouseSync(sync);
                        _executionLogService.InsertExecutionLog(log);

                        transaction.Commit();
                    }
                }
            }
        }

    }
}
