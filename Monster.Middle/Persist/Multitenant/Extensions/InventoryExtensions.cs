using System.Collections.Generic;
using System.Linq;
using Monster.Acumatica.Api.Distribution;
using Push.Shopify.Api.Inventory;

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
        
    }
}
