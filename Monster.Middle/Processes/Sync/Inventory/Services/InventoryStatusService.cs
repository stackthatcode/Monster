using System.Linq;
using Monster.Middle.Persist.Multitenant.Sync;
using Monster.Middle.Processes.Sync.Extensions;
using Monster.Middle.Processes.Sync.Inventory.Model;
using Monster.Middle.Processes.Sync.Persist;

namespace Monster.Middle.Processes.Sync.Inventory.Services
{
    public class InventoryStatusService
    {
        private readonly SyncInventoryRepository _repository;

        public InventoryStatusService(SyncInventoryRepository repository)
        {
            _repository = repository;
        }


        public WarehouseSyncState GetWarehouseSyncStatus()
        {
            var warehouses = _repository.RetrieveWarehouses();
            var locations = _repository.RetrieveLocations();

            var output = new WarehouseSyncState();

            output.UnmatchedAcumaticaWarehouses
                = warehouses
                    .Unmatched()
                    .Select(x => x.AcumaticaWarehouseId)
                    .ToList();

            output.MismatchedWarehouseLocations
                = warehouses
                    .Mismatched()
                    .Select(x => x.AcumaticaWarehouseId)
                    .ToList();

            output.UnmatchedShopifyLocations
                = locations
                    .Unmatched()
                    .Select(x => x.ShopifyLocationName)
                    .ToList();

            output.MatchedWarehouseLocations
                = locations
                    .Matched()
                    .Select(x => x.ShopifyLocationName)
                    .ToList();

            return output;
        }
    }
}
