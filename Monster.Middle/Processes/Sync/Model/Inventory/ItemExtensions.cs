using System;
using System.Collections.Generic;
using System.Linq;
using Monster.Middle.Persist.Instance;
using Monster.Middle.Processes.Sync.Misc;

namespace Monster.Middle.Processes.Sync.Model.Inventory
{
    public static class ItemExtensions
    {
        public static AcumaticaStockItem MatchedStockItem(this ShopifyVariant input)
        {
            return input.ShopAcuItemSyncs?.First().AcumaticaStockItem;
        }

        public static bool HasMatch(this AcumaticaStockItem input)
        {
            return input.ShopAcuItemSyncs.Any();
        }

        public static ShopifyVariant MatchedVariant(this AcumaticaStockItem stockItem)
        {
            return stockItem.ShopAcuItemSyncs?.First().ShopifyVariant;
        }


        public static bool IsSynced(this ShopifyVariant input)
        {
            return input.ShopAcuItemSyncs.Any();
        }

        public static bool AreSkuAndItemIdMismatched(this ShopifyVariant input)
        {
            if (!input.IsSynced())
            {
                return false;
            }
            else
            {
                return input.ShopifySku.StandardizedSku() != input.MatchedStockItem().ItemId.StandardizedSku();
            }
        }

        public static bool? IsTaxable(this AcumaticaStockItem input, MonsterSetting settings)
        {
            if (input.AcumaticaTaxCategory == settings.AcumaticaTaxableCategory)
            {
                return true;
            }

            if (input.AcumaticaTaxCategory == settings.AcumaticaTaxExemptCategory)
            {
                return false;
            }

            return null;
        }

        public static string TaxCategory(this bool isTaxable, MonsterSetting settings)
        {
            return isTaxable ? settings.AcumaticaTaxableCategory : settings.AcumaticaTaxExemptCategory;
        }


        public static string YesNoNAPlainEnglish(this bool? input)
        {
            if (input.HasValue)
            {
                return input.Value ? "YES" : "NO";
            }
            else
            {
                return "N/A";
            }
        }

        public static bool AreTaxesMismatched(this ShopifyVariant input, MonsterSetting settings)
        {
            if (!input.IsSynced())
            {
                return false;
            }

            if (input.ShopifyIsTaxable 
                && input.MatchedStockItem().AcumaticaTaxCategory == settings.AcumaticaTaxableCategory)
            {
                return false;
            }

            if (!input.ShopifyIsTaxable 
                && input.MatchedStockItem().AcumaticaTaxCategory == settings.AcumaticaTaxExemptCategory)
            {
                return false;
            }

            return true;
        }


        public static List<AcumaticaWarehouseDetail> WarehouseDetails(this AcumaticaStockItem input, string warehouseId)
        {
            return input.AcumaticaWarehouseDetails
                        .Where(x => x.AcumaticaWarehouseId == warehouseId)
                        .ToList();
        }

        public static List<AcumaticaWarehouseDetail> WarehouseDetails(this AcumaticaStockItem input, List<string> warehouseIds)
        {
            return input.AcumaticaWarehouseDetails
                .Where(x => warehouseIds.Contains(x.AcumaticaWarehouseId))
                .ToList();
        }

    }
}
