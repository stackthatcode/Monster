using System.Collections.Generic;
using System.Linq;
using Monster.Middle.Persist.Instance;
using Push.Shopify.Api.Transactions;

namespace Monster.Middle.Processes.Shopify.Persist
{
    public static class TransactionExtensions
    {
        public static bool IsMatch(
                this ShopifyTransaction input, 
                ShopifyTransaction other)
        {
            return input.ShopifyTransactionId == other.ShopifyTransactionId;
        }

        public static bool AnyMatch(this IEnumerable<ShopifyTransaction> input, ShopifyTransaction other)
        {
            return input.Any(x => x.IsMatch(other));
        }

        public static ShopifyTransaction Match(this IEnumerable<ShopifyTransaction> input, ShopifyTransaction other)
        {
            return input.FirstOrDefault(x => x.IsMatch(other));
        }

        public static long CustomerId(this ShopifyTransaction transactionRecord)
        {
            return transactionRecord.ShopifyOrder.ShopifyCustomer.ShopifyCustomerId;
        }

        public static long OrderId(this ShopifyTransaction transactionRecord)
        {
            return transactionRecord.ShopifyOrder.ShopifyOrderId;
        }

        public static ShopifyTransaction ActualPaymentTransaction(this ShopifyOrder order)
        {
            return order.ShopifyTransactions.FirstOrDefault(
                x => (x.ShopifyKind == TransactionKind.Capture || x.ShopifyKind == TransactionKind.Sale)
                     && x.ShopifyStatus == TransactionStatus.Success);
        }
    }
}
