using System.Collections.Generic;
using System.Linq;
using Push.Foundation.Utilities.Json;
using Push.Shopify.Api.Product;


namespace Monster.Middle.Persist.Multitenant.Shopify
{
    public static class InventoryExtensions
    {       
        public static bool IsPaid(this UsrShopifyOrder order)
        {
            return
                order.ShopifyFinancialStatus == FinancialStatus.Paid ||
                order.ShopifyFinancialStatus == FinancialStatus.PartiallyRefunded ||
                order.ShopifyFinancialStatus == FinancialStatus.Refunded;
        }


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

        public static IList<UsrShopifyVariant> 
                    ExcludeUnmatched(this IEnumerable<UsrShopifyVariant> input)
        {
            return input
                    .Where(x => !x.UsrShopAcuItemSyncs.Any())
                    .ToList();
        }
        
        public static IList<UsrShopifyVariant> 
                ExcludeMatched(this IEnumerable<UsrShopifyVariant> input)
        {
            return input
                .Where(x => x.UsrShopAcuItemSyncs.Any())
                .ToList();
        }

        public static bool IsMatched(this UsrShopifyVariant variant)
        {
            return variant.UsrShopAcuItemSyncs.Any();
        }

        public static bool IsNotMatched(this UsrShopifyVariant variant)
        {
            return !variant.IsMatched();
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


        public static int ControlQty(
                this IEnumerable<UsrShopifyInventoryLevel> input)
        {
            return input.Sum(x => x.ShopifyAvailableQuantity);

        }
        public static double ControlCost(
                this IEnumerable<UsrShopifyInventoryLevel> input,
                double defaultCogs)
        {
            return input.Sum(
                x =>
                    x.UsrShopifyVariant.CogsByMarginPercent(defaultCogs)
                    * x.ShopifyAvailableQuantity);
        }
    }
}
