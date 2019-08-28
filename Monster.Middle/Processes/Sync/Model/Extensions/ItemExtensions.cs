using System.Collections.Generic;
using System.Linq;
using Monster.Middle.Persist.Instance;

namespace Monster.Middle.Processes.Sync.Model.Extensions
{
    public static class ItemExtensions
    {
        public static AcumaticaStockItem 
                    MatchedStockItem(this ShopifyVariant input)
        {
            return input.ShopAcuItemSyncs?.First().AcumaticaStockItem;
        }

        public static List<AcumaticaWarehouseDetail> 
                    WarehouseDetails(this AcumaticaStockItem input, string warehouseId)
        {
            return input.AcumaticaWarehouseDetails
                        .Where(x => x.AcumaticaWarehouseId == warehouseId)
                        .ToList();
        }

        public static List<AcumaticaWarehouseDetail>
                    WarehouseDetails(this AcumaticaStockItem input, List<string> warehouseIds)
        {
            return input.AcumaticaWarehouseDetails
                .Where(x => warehouseIds.Contains(x.AcumaticaWarehouseId))
                .ToList();
        }

        public static ShopifyVariant MatchedVariant(this AcumaticaStockItem input)
        {
            return input
                .ShopAcuItemSyncs?.First().ShopifyVariant;
        }

        public static bool HasMatch(this AcumaticaStockItem input)
        {
            return input.ShopAcuItemSyncs.Any();
        }
    }
}
