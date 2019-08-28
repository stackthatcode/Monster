using System.Collections.Generic;
using System.Linq;
using Monster.Acumatica.Api.Distribution;
using Monster.Middle.Persist.Instance;
using Monster.Middle.Processes.Sync.Model.Misc;
using Push.Shopify.Api.Inventory;

namespace Monster.Middle.Processes.Sync.Model.Extensions
{
    public static class LocationExtensions
    {
        public static ShopifyLocation FindByShopifyId(
            this IList<ShopifyLocation> persistLocations,
            Location location)
        {
            return persistLocations
                .FirstOrDefault(x => x.ShopifyLocationId == location.id);
        }

        public static AcumaticaWarehouse FindByAcumaticaId(
            this IList<AcumaticaWarehouse> persistWarehouses,
            Warehouse warehouse)
        {
            return persistWarehouses
                .FirstOrDefault(x => x.AcumaticaWarehouseId == warehouse.WarehouseID.value);
        }

        public static bool AutoMatches(
                this AcumaticaWarehouse warehouse, 
                ShopifyLocation location)
        {
            return warehouse.AcumaticaWarehouseId == location.StandardizedName();
        }

        public static bool MatchesIdWithName(
                this ShopifyLocation location, 
                AcumaticaWarehouse warehouse)
        {
            return warehouse.AcumaticaWarehouseId == location.StandardizedName();
        }

        public static bool IsImproperlyMatched(this ShopifyLocation location)
        {
            var warehouse = location.MatchedWarehouse();
            if (warehouse == null)
            {
                return false;
            }

            return !location.MatchesIdWithName(warehouse);
        }

        public static bool HasMatch(this AcumaticaWarehouse warehouse)
        {
            return warehouse.ShopAcuWarehouseSyncs.Any();
        }

        public static bool HasMatch(this ShopifyLocation location)
        {
            return location.ShopAcuWarehouseSyncs.Any();
        }

        public static bool HasNoMatch(this ShopifyLocation location)
        {
            return !location.HasMatch();
        }


        public static 
                IList<AcumaticaWarehouse> Unmatched(
                    this IEnumerable<AcumaticaWarehouse> input)
        {
            return input
                .Where(x => x.ShopAcuWarehouseSyncs.Count == 0)
                .ToList();
        }
        

        public static IList<ShopifyLocation>
                Unmatched(this IEnumerable<ShopifyLocation> input)
        {
            return input.Where(x => x.HasNoMatch()).ToList();
        }

        public static IList<ShopifyLocation>
                Matched(this IEnumerable<ShopifyLocation> input)
        {
            return input.Where(x => x.HasMatch()).ToList();
        }

        
        public static ShopifyLocation 
                MatchedLocation(this AcumaticaWarehouse input)
        {
            return input
                .ShopAcuWarehouseSyncs
                .FirstOrDefault()?.ShopifyLocation;
        }

        public static AcumaticaWarehouse
                MatchedWarehouse(this ShopifyLocation input)
        {
            return input
                .ShopAcuWarehouseSyncs
                .FirstOrDefault()?.AcumaticaWarehouse;
        }
    }
}

