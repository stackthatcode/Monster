using System;
using System.Collections.Generic;
using System.Linq;
using Push.Foundation.Utilities.Json;
using Push.Shopify.Api.Product;

namespace Monster.Middle.Persist.Multitenant.Extensions
{
    public static class InventoryExtensions
    {
        public static IList<UsrShopifyVariant> Exclude(
                    this IEnumerable<UsrShopifyVariant> input, 
                    UsrShopifyVariant exclude)
        {
            return input
                .Where(x => x.ShopifyVariantId != exclude.ShopifyVariantId &&
                                    x.ShopifySku != exclude.ShopifySku).ToList();
        }
        
        public static IList<UsrShopifyVariant> ExcludeMissing(
                    this IEnumerable<UsrShopifyVariant> input)
        {
            return input
                    .Where(x => x.IsMissing == false)
                    .ToList();
        }

        public static IList<UsrShopifyVariant> ExcludeUnmatched(
                    this IEnumerable<UsrShopifyVariant> input)
        {
            return input
                    .Where(x => !x.UsrAcumaticaStockItems.Any())
                    .ToList();
        }
        
        public static IList<UsrShopifyVariant> 
                ExcludeMatched(this IEnumerable<UsrShopifyVariant> input)
        {
            return input
                .Where(x => x.UsrAcumaticaStockItems.Any())
                .ToList();
        }

        public static bool IsMatched(this UsrShopifyVariant variant)
        {
            return variant.UsrAcumaticaStockItems.Any();
        }

        public static bool IsNotMatched(this UsrShopifyVariant variant)
        {
            return !variant.IsMatched();
        }

        //public static IList<UsrShopifyInventoryLevel> By

        public static bool IsMatchedToShopify(
                    this UsrAcumaticaStockItem stockItem)
        {
            return stockItem.ShopifyVariantMonsterId != null;
        }

        public static double CogsByMarginPercent(
                    this UsrShopifyVariant variant, double marginPercent)
        {
            var variantObject 
                = variant.ShopifyVariantJson.DeserializeFromJson<Variant>();

            return variantObject.price * marginPercent;
        }

        public static List<UsrShopifyInventoryLevel> ByParentId(
                        this IEnumerable<UsrShopifyInventoryLevel> input, 
                        long parentMonsterId)
        {
            return input
                .Where(x => 
                    x.UsrShopifyVariant.ParentMonsterId == parentMonsterId)
                .ToList();
        }

        public static string AcumaticaStockItemId(
                        this UsrShopifyInventoryLevel input)
        {
            var stockItems = input
                .UsrShopifyVariant
                .UsrAcumaticaStockItems;

            if (stockItems.Count == 0)
            {
                throw new Exception("No matching Acumatica Stock Items");
            }

            return stockItems.First().ItemId;
        }

        public static string AcumaticaWarehouseId(
                        this UsrShopifyInventoryLevel input)
        {
            var location = input.UsrShopifyLocation;
            if (location == null)
            {
                throw new Exception("Inventory Level not assigned to Location");
            }

            var warehouse = location.UsrAcumaticaWarehouses.FirstOrDefault();
            if (warehouse == null)
            {
                throw new Exception("Shopify Location not matched to Acumatica Warehouse");
            }

            return warehouse.AcumaticaWarehouseId;
        }
    }
}
