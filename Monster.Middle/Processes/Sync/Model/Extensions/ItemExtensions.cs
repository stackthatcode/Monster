using System.Collections.Generic;
using System.Linq;
using Monster.Middle.Persist.Instance;

namespace Monster.Middle.Processes.Sync.Model.Extensions
{
    public static class ItemExtensions
    {
        public static UsrAcumaticaStockItem 
                    MatchedStockItem(this UsrShopifyVariant input)
        {
            return input.UsrShopAcuItemSyncs?.First().UsrAcumaticaStockItem;
        }

        public static List<UsrAcumaticaWarehouseDetail> 
                    WarehouseDetails(this UsrAcumaticaStockItem input, string warehouseId)
        {
            return input.UsrAcumaticaWarehouseDetails
                        .Where(x => x.AcumaticaWarehouseId == warehouseId)
                        .ToList();
        }

        public static List<UsrAcumaticaWarehouseDetail>
                    WarehouseDetails(this UsrAcumaticaStockItem input, List<string> warehouseIds)
        {
            return input.UsrAcumaticaWarehouseDetails
                .Where(x => warehouseIds.Contains(x.AcumaticaWarehouseId))
                .ToList();
        }

        public static UsrShopifyVariant MatchedVariant(this UsrAcumaticaStockItem input)
        {
            return input
                .UsrShopAcuItemSyncs?.First().UsrShopifyVariant;
        }

        public static bool HasMatch(this UsrAcumaticaStockItem input)
        {
            return input.UsrShopAcuItemSyncs.Any();
        }
    }
}
