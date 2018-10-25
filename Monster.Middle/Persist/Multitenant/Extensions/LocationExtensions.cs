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

        public static bool AutoMatches(
                    this UsrAcumaticaWarehouse warehouse, 
                    UsrShopifyLocation location)
        {
            return warehouse.AcumaticaWarehouseId == location.StandardizedName();
        }

        public static bool MatchesIdWithName(
                    this UsrShopifyLocation location, 
                    UsrAcumaticaWarehouse warehouse)
        {
            return warehouse.AcumaticaWarehouseId == location.StandardizedName();
        }
        
        public static bool IsMatched(this UsrShopifyLocation location)
        {
            return location.UsrAcumaticaWarehouses.Any();
        }

        public static bool IsUnmatched(this UsrShopifyLocation location)
        {
            return !location.IsMatched();
        }


        public static IList<UsrAcumaticaWarehouse> 
                Unmatched(this IEnumerable<UsrAcumaticaWarehouse> input)
        {
            return input
                .Where(x => x.ShopifyLocationMonsterId == null)
                .ToList();
        }

        public static IList<UsrAcumaticaWarehouse>
                Mismatched(this IEnumerable<UsrAcumaticaWarehouse> input)
        {
            return input
                .Where(x => x.IsNameMismatched)
                .ToList();
        }


        public static IList<UsrShopifyLocation>
            Unmatched(this IEnumerable<UsrShopifyLocation> input)
        {
            return input
                .Where(x => x.IsUnmatched())
                .ToList();
        }
    }
}
