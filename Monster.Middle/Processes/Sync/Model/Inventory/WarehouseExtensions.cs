using System;
using System.Collections.Generic;
using System.Linq;
using Monster.Middle.Persist.Instance;

namespace Monster.Middle.Processes.Sync.Model.Inventory
{
    public static class WarehouseExtensions
    {
        public static string AcumaticaStockItemId(this ShopifyVariant input)
        {
            if (!input.AcumaticaStockItems.Any())
            {
                throw new Exception("No synchronized Acumatica Stock Items");
            }

            return input.AcumaticaStockItems.First().ItemId;
        }

        public static string AcumaticaWarehouseId(this ShopifyLocation input)
        {
            if (input.ShopAcuWarehouseSyncs.Count == 0)
            {
                throw new Exception("Inventory Level not assigned to Location");
            }

            return input
                .ShopAcuWarehouseSyncs
                .First()
                .AcumaticaWarehouse
                .AcumaticaWarehouseId;
        }

        public static List<AcumaticaWarehouse> 
                    MatchedWarehouses(this ShopifyLocation location)
        {
            return location
                .ShopAcuWarehouseSyncs
                .Select(x => x.AcumaticaWarehouse)
                .ToList();
        }

        public static List<string> MatchedWarehouseIds(this ShopifyLocation location)
        {
            return location.MatchedWarehouses()
                .Select(x => x.AcumaticaWarehouseId)
                .ToList();
        }
    }
}

