using System.Collections.Generic;
using System.Linq;
using Monster.Middle.Persist.Instance;
using Monster.Middle.Processes.Sync.Misc;

namespace Monster.Middle.Processes.Sync.Model.Orders
{
    public static class ItemExtensions
    {
        public static AcumaticaStockItem MatchedStockItem(this ShopifyVariant input)
        {
            return input.ShopAcuItemSyncs?.First().AcumaticaStockItem;
        }

        public static bool IsSynced(this ShopifyVariant input)
        {
            return input.MatchedStockItem() != null;
        }

        public static bool AreSkuAndItemIdMatched(this ShopifyVariant input)
        {
            if (!input.IsSynced())
            {
                return false;
            }
            else
            {
                return input.ShopifySku.StandardizedSku() == input.MatchedStockItem().ItemId.StandardizedSku();
            }
        }

        public static bool AreTaxesMatched(this ShopifyVariant input, MonsterSetting settings)
        {
            if (!input.IsSynced())
            {
                return false;
            }

            if (input.ShopifyIsTaxable 
                && input.MatchedStockItem().AcumaticaTaxCategory == settings.AcumaticaTaxableCategory)
            {
                return true;
            }

            if (!input.ShopifyIsTaxable 
                && input.MatchedStockItem().AcumaticaTaxCategory == settings.AcumaticaTaxExemptCategory)
            {
                return true;
            }

            return false;
        }


        public static List<AcumaticaWarehouseDetail> WarehouseDetails(this AcumaticaStockItem input, string warehouseId)
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
