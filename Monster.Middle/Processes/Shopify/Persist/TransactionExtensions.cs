using System.Collections.Generic;
using System.Linq;
using Monster.Middle.Persist.Multitenant;

namespace Monster.Middle.Processes.Shopify.Persist
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

        public static UsrShopifyTransaction Match(
                    this IEnumerable<UsrShopifyTransaction> input,
                    UsrShopifyTransaction other)
        {
            return input.FirstOrDefault(x => x.IsMatch(other));
        }

        public static long CustomerId(this UsrShopifyTransaction transactionRecord)
        {
            return transactionRecord.UsrShopifyOrder.UsrShopifyCustomer.ShopifyCustomerId;
        }

        public static long OrderId(this UsrShopifyTransaction transactionRecord)
        {
            return transactionRecord.UsrShopifyOrder.ShopifyOrderId;
        }

    }
}
