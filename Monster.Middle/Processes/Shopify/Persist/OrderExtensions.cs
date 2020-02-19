using System.Linq;
using Monster.Middle.Persist.Instance;
using Push.Foundation.Utilities.Json;
using Push.Shopify.Api.Order;
using Push.Shopify.Api.Transactions;


namespace Monster.Middle.Processes.Shopify.Persist
{
    public static class OrderExtensions
    {
        public static bool StatusChangeDetected(this ShopifyOrder currentRecord, Order newOrder)
        {
            return
                currentRecord.ShopifyIsCancelled != newOrder.IsCancelled ||
                currentRecord.ShopifyFinancialStatus != newOrder.financial_status;
        }

        public static bool IsCancelledOrAllRefunded(this ShopifyOrder orderRecord)
        {
            return orderRecord.ShopifyAreAllItemsRefunded || orderRecord.ShopifyIsCancelled;
        }


        public static bool IsNotCancelledOrAllRefunded(this ShopifyOrder orderRecord)
        {
            return !orderRecord.IsCancelledOrAllRefunded();
        }
    }
}

