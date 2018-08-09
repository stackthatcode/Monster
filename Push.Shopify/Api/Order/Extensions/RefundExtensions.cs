using System.Linq;


namespace Push.Shopify.Api.Order.Extensions
{
    public static class RefundExtensions
    {
        public static bool IsValid(this Refund refund)
        {
            return refund.transactions.Any() 
                    && refund.transactions.All(x => x.IsSuccess());
        }

        public static decimal TransactionAmount(this Refund refund)
        {
            return refund.transactions.Sum(x => x.amount);
        }
        
        public static decimal ShippingAdjustments(this Refund refund)
        {
            return refund.order_adjustments
                        .Where(x => x.IsShippingAdjustment())
                        .Sum(x => x.amount);
        }

        public static decimal RefundAdjustments(this Refund refund)
        {
            return refund.order_adjustments
                        .Where(x => x.IsRefundDiscrepancy())
                        .Sum(x => x.amount);
        }
    }
}


