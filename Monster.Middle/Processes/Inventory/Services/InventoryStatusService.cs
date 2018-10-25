using System.Linq;
using Monster.Middle.Persist.Multitenant;
using Monster.Middle.Persist.Multitenant.Extensions;
using Monster.Middle.Processes.Inventory.Model;

namespace Monster.Middle.Processes.Inventory.Services
{
    public class InventoryStatusService
    {
        private readonly InventoryRepository _inventoryRepository;
        private readonly LocationRepository _locationRepository;

        public InventoryStatusService(
                InventoryRepository inventoryRepository, 
                LocationRepository locationRepository)
        {
            _inventoryRepository = inventoryRepository;
            _locationRepository = locationRepository;
        }


        public LocationStatus GetCurrentLocationStatus()
        {
            var warehouses =
                    _locationRepository.RetreiveAcumaticaWarehouses();
            var locations =
                    _locationRepository.RetreiveShopifyLocations();

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
