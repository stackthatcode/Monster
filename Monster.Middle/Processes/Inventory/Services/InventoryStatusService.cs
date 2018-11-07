using System.Linq;
using Monster.Middle.Persist.Multitenant;
using Monster.Middle.Persist.Multitenant.Acumatica;
using Monster.Middle.Persist.Multitenant.Extensions;
using Monster.Middle.Persist.Multitenant.Shopify;
using Monster.Middle.Processes.Inventory.Model;

namespace Monster.Middle.Processes.Inventory.Services
{
    public class InventoryStatusService
    {
        private readonly 
            ShopifyInventoryRepository _shopifyInventoryRepository;
        private readonly 
            AcumaticaInventoryRepository _acumaticaInventoryRepository;

        public InventoryStatusService(
                ShopifyInventoryRepository shopifyInventoryRepository,
                AcumaticaInventoryRepository acumaticaInventoryRepository)
        {
            _shopifyInventoryRepository = shopifyInventoryRepository;
            _acumaticaInventoryRepository = acumaticaInventoryRepository;
        }


        public LocationStatus GetCurrentLocationStatus()
        {
            var warehouses =
                    _acumaticaInventoryRepository.RetreiveWarehouses();
            var locations =
                    _shopifyInventoryRepository.RetreiveLocations();

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
