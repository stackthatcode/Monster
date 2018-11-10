using System.Collections.Generic;
using System.Linq;
using Monster.Middle.Processes.Inventory;

namespace Monster.Middle.Persist.Multitenant.Sync
{
    public static class LocationExtensions
    {        
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

        public static bool IsMatched(this UsrAcumaticaWarehouse warehouse)
        {
            return warehouse.UsrShopAcuWarehouseSyncs.Any();
        }

        public static bool IsMatched(this UsrShopifyLocation location)
        {
            return location.UsrShopAcuWarehouseSyncs.Any();
        }

        public static bool IsUnmatched(this UsrShopifyLocation location)
        {
            return !location.IsMatched();
        }


        public static 
                IList<UsrAcumaticaWarehouse> Unmatched(
                    this IEnumerable<UsrAcumaticaWarehouse> input)
        {
            return input
                .Where(x => x.UsrShopAcuWarehouseSyncs == null)
                .ToList();
        }

        public static 
                IList<UsrAcumaticaWarehouse> Mismatched(
                    this IEnumerable<UsrAcumaticaWarehouse> input)
        {
            return input
                .Where(x => x.UsrShopAcuWarehouseSyncs.Any()
                            && x.UsrShopAcuWarehouseSyncs.First().IsNameMismatched)
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

