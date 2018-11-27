using System.Linq;
using Monster.Middle.Persist.Multitenant.Sync;
using Monster.Middle.Processes.Sync.Extensions;
using Monster.Middle.Processes.Sync.Inventory.Model;

namespace Monster.Middle.Processes.Inventory.Services
{
    public class InventoryStatusService
    {
        private readonly SyncInventoryRepository _repository;

        public InventoryStatusService(SyncInventoryRepository repository)
        {
            _repository = repository;
        }


        public LocationStatus GetCurrentLocationStatus()
        {
            var warehouses = _repository.RetrieveWarehouses();
            var locations = _repository.RetrieveLocations();

            var output = new LocationStatus();

            output.UnmatchedWarehouses
                = warehouses
                    .Unmatched()
                    .Select(x => x.AcumaticaWarehouseId)
                    .ToList();

            output.MismatchedWarehouses
                = warehouses
                    .Mismatched()
                    .Select(x => x.AcumaticaWarehouseId)
                    .ToList();

            output.UnmatchedLocations
                = locations
                    .Unmatched()
                    .Select(x => x.ShopifyLocationName)
                    .ToList();

            return output;
        }
    }
}
