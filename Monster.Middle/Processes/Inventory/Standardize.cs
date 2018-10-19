using Monster.Middle.Persist.Multitenant;
using Push.Shopify.Api.Product;

namespace Monster.Middle.Processes.Inventory
{
    public class Standardize
    {
        public static string StockItemTitle(Product product, Variant variant)
        {
            var output = product.title + " - ";
            output += 
                variant.title != "Default Title"
                    ? variant.title 
                    : variant.sku;
                
            return output;
        }

        public static string Sku(string sku)
        {
            return sku.ToUpper();
        }
    }

    public static class StandardizeExtensions
    {
        public static string StandardizedSku(this UsrShopifyVariant input)
        {
            return Standardize.Sku(input.ShopifySku);
        }
    }
}
