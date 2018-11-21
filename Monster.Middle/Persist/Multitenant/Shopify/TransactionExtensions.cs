using System.Collections.Generic;
using System.Linq;

namespace Monster.Middle.Persist.Multitenant.Shopify
{
    public static class TransactionExtensions
    {
        public static bool IsMatch(
                this UsrShopifyTransaction input, 
                UsrShopifyTransaction other)
        {
            return input.ShopifyTransactionId == other.ShopifyTransactionId;
        }

        public static bool AnyMatch(
                this IEnumerable<UsrShopifyTransaction> input,
                UsrShopifyTransaction other)
        {
            return input.Any(x => x.IsMatch(other));
        }
    }
}
