using System.Collections.Generic;
using System.Linq;
using Monster.Middle.Persist.Instance;
using Push.Foundation.Utilities.Json;
using Push.Shopify.Api.Product;

namespace Monster.Middle.Processes.Shopify.Persist
{
    public static class InventoryExtensions
    {       
        public static bool IsPaid(this ShopifyOrder order)
        {
            return
                order.ShopifyFinancialStatus == FinancialStatus.Paid ||
                order.ShopifyFinancialStatus == FinancialStatus.PartiallyRefunded ||
                order.ShopifyFinancialStatus == FinancialStatus.Refunded;
        }


        public static IList<ShopifyVariant> Exclude(
                    this IEnumerable<ShopifyVariant> input, 
                    ShopifyVariant exclude)
        {
            return input
                .Where(x => x.ShopifyVariantId != exclude.ShopifyVariantId &&
                                    x.ShopifySku != exclude.ShopifySku).ToList();
        }
        
        public static IList<ShopifyVariant> ExcludeMissing(
                    this IEnumerable<ShopifyVariant> input)
        {
            return input
                    .Where(x => x.IsMissing == false)
                    .ToList();
        }
        
        public static IList<ShopifyVariant> ExcludeMatched(this IEnumerable<ShopifyVariant> input)
        {
            return input
                .Where(x => !x.AcumaticaStockItems.Any())
                .ToList();
        }

        public static ShopifyInventoryLevel 
                    InventoryLevel(this ShopifyVariant input, long locationId)
        {
            return input
                .ShopifyInventoryLevels
                .FirstOrDefault(x => x.ShopifyLocationId == locationId);
        }

        public static List<ShopifyVariant> NonMissingVariants(this ShopifyProduct product)
        {
            return product.ShopifyVariants.Where(x => !x.IsMissing).ToList();
        }

        public static List<ShopifyInventoryLevel>
                    WithMatchedVariants(this IEnumerable<ShopifyInventoryLevel> input)
        {
            return input.Where(x => x.ShopifyVariant.IsMatched()).ToList();
        }


        public static bool IsMatched(this ShopifyVariant variant)
        {
            return variant.AcumaticaStockItems.Any();
        }

        public static bool IsNotMatched(this ShopifyVariant variant)
        {
            return !variant.IsMatched();
        }

        public static double CogsByMarginPercent(this Variant variantObject, double marginPercent)
        {
            return variantObject.price * marginPercent;
        }

        public static double CogsControlTotal(this List<ShopifyInventoryLevel> inventory)
        {
            return (double)inventory.Sum(
                x => x.ShopifyAvailableQuantity * x.ShopifyVariant.ShopifyCost);
        }


        public static List<ShopifyInventoryLevel> ByParentProductId(
                        this IEnumerable<ShopifyInventoryLevel> input, long parentMonsterId)
        {
            return input
                .Where(x => x.ShopifyVariant.ParentMonsterId == parentMonsterId)
                .ToList();
        }


        public static int ControlQty(this IEnumerable<ShopifyInventoryLevel> input)
        {
            return input.Sum(x => x.ShopifyAvailableQuantity);
        }
    }
}
