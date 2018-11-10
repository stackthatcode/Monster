using System.Collections.Generic;
using System.Linq;
using Monster.Acumatica.Api.Distribution;
using Monster.Middle.Processes.Inventory;
using Push.Shopify.Api.Inventory;

namespace Monster.Middle.Persist.Multitenant.Extensions
{
    public static class LocationExtensions
    {
        public static UsrShopifyLocation FindByShopifyId(
                    this IList<UsrShopifyLocation> persistLocations, 
                    Location location)
        {
            return persistLocations
                .FirstOrDefault(x => x.ShopifyLocationId == location.id);
        }

        public static UsrAcumaticaWarehouse FindByAcumaticaId(
                    this IList<UsrAcumaticaWarehouse> persistWarehouses, 
                    Warehouse warehouse)
        {
            return persistWarehouses
                .FirstOrDefault(x => x.AcumaticaWarehouseId == warehouse.WarehouseID.value);
        }

    }
}
