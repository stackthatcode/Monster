using System.Text.RegularExpressions;
using Monster.Middle.Persist.Instance;
using Push.Shopify.Api.Product;

namespace Monster.Middle.Processes.Sync.Misc
{
    public static class Canonizers
    {
        public static string StandardizedStockItemTitle(Product product, Variant variant)
        {
            var output = product.title + " - ";
            output += 
                variant.title != "Default Title"
                    ? variant.title 
                    : variant.sku;
                
            return output;
        }

        public static string StandardizedSku(this string sku)
        {
            return sku.ToUpper();
        }

        public static string StandardizeLocationName(this string name)
        {
            var output = name.ToUpper();
            var rgx = new Regex("[^a-zA-Z0-9]");
            output = rgx.Replace(output, "");
            return output;
        }
    }

    public static class CanonizerExtensions
    {
        public static string StandardizedSku(this ShopifyVariant input)
        {
            return Canonizers.StandardizedSku(input.ShopifySku);
        }

        public static string StandardizedName(this ShopifyLocation input)
        {
            return Canonizers.StandardizeLocationName(input.ShopifyLocationName);
        }
    }
}
