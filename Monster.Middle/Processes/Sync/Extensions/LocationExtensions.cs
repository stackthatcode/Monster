using System.Collections.Generic;
using System.Linq;
using Monster.Acumatica.Api.Distribution;
using Monster.Middle.Persist.Multitenant;
using Monster.Middle.Processes.Sync.Inventory.Model;
using Push.Shopify.Api.Inventory;

namespace Monster.Middle.Processes.Sync.Extensions
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

        public static bool IsImproperlyMatched(this UsrShopifyLocation location)
        {
            var warehouse = location.MatchedWarehouse();
            if (warehouse == null)
            {
                return false;
            }

            return !location.MatchesIdWithName(warehouse);
        }

        public static bool HasMatch(this UsrAcumaticaWarehouse warehouse)
        {
            return warehouse.UsrShopAcuWarehouseSyncs.Any();
        }

        public static bool HasMatch(this UsrShopifyLocation location)
        {
            return location.UsrShopAcuWarehouseSyncs.Any();
        }

        public static bool HasNoMatch(this UsrShopifyLocation location)
        {
            return !location.HasMatch();
        }


        public static 
                IList<UsrAcumaticaWarehouse> Unmatched(
                    this IEnumerable<UsrAcumaticaWarehouse> input)
        {
            return input
                .Where(x => x.UsrShopAcuWarehouseSyncs.Count == 0)
                .ToList();
        }
        

        public static IList<UsrShopifyLocation>
                Unmatched(this IEnumerable<UsrShopifyLocation> input)
        {
            return input.Where(x => x.HasNoMatch()).ToList();
        }

        public static IList<UsrShopifyLocation>
                Matched(this IEnumerable<UsrShopifyLocation> input)
        {
            return input.Where(x => x.HasMatch()).ToList();
        }

        
        public static UsrShopifyLocation 
                MatchedLocation(this UsrAcumaticaWarehouse input)
        {
            return input
                .UsrShopAcuWarehouseSyncs
                .FirstOrDefault()?.UsrShopifyLocation;
        }

        public static UsrAcumaticaWarehouse
                MatchedWarehouse(this UsrShopifyLocation input)
        {
            return input
                .UsrShopAcuWarehouseSyncs
                .FirstOrDefault()?.UsrAcumaticaWarehouse;
        }
    }
}

