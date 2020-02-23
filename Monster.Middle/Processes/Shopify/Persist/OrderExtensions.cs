using System.Linq;
using Monster.Middle.Persist.Instance;
using Push.Shopify.Api.Order;


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

        public static ShopifyRefund Refund(this ShopifyOrder orderRecord, long shopifyRefundId)
        {
            return orderRecord.ShopifyRefunds.FirstOrDefault(x => x.ShopifyRefundId == shopifyRefundId);
        }
    }
}

