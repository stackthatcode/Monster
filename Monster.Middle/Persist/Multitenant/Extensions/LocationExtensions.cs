using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Monster.Acumatica.Api.Distribution;
using Push.Shopify.Api.Inventory;

namespace Monster.Middle.Persist.Multitenant.Extensions
{
    public static class LocationExtensions
    {
        public static UsrShopifyLocation FindByShopifyId(
                this IList<UsrShopifyLocation> persistLocations, Location location)
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

        public static bool Matches(
                this UsrAcumaticaWarehouse warehouse, UsrShopifyLocation location)
        {
            return warehouse.AcumaticaWarehouseId == location.ShopifyLocationName;
        }

        public static bool Matches(
                this UsrShopifyLocation location, UsrAcumaticaWarehouse warehouse)
        {
            return warehouse.AcumaticaWarehouseId == location.ShopifyLocationName;
        }


        public static bool IsMatched(this UsrShopifyLocation location)
        {
            return location.UsrAcumaticaWarehouses.Any();
        }

        public static bool IsUnmatched(this UsrShopifyLocation location)
        {
            return !location.IsMatched();
        }
    }
}
