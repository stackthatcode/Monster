using System.Collections.Generic;
using System.Linq;
using Monster.Middle.Persist.Instance;
using Push.Shopify.Api.Transactions;

namespace Monster.Middle.Processes.Shopify.Persist
{
    public static class TransactionExtensions
    {
        public static bool IsMatch(this ShopifyTransaction input, ShopifyTransaction other)
        {
            return input.ShopifyTransactionId == other.ShopifyTransactionId;
        }

        public static ShopifyTransaction Match(
                this IEnumerable<ShopifyTransaction> input, ShopifyTransaction other)
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


        public static ShopifyTransaction PaymentTransaction(this ShopifyOrder order)
        {
            return order.ShopifyTransactions.FirstOrDefault(
                x => (x.ShopifyKind == TransactionKind.Capture 
                        || x.ShopifyKind == TransactionKind.Sale)
                        && x.ShopifyStatus == TransactionStatus.Success);
        }

        public static bool DontIgnoreForSync(this Transaction transaction)
        {
            return transaction.gateway != Gateway.Manual
                   && transaction.status == TransactionStatus.Success
                   && (transaction.kind == TransactionKind.Capture
                       || transaction.kind == TransactionKind.Sale
                       || transaction.kind == TransactionKind.Refund);
        }

        public static bool IgnoreForSync(this Transaction transaction)
        {
            return transaction.DontIgnoreForSync();
        }
    }
}
