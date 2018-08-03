using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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


        public static decimal ShippingAdjustment(this Refund refund)
        {
            return refund.order_adjustments
                        .Where(x => x.IsShippingAdjustment())
                        .Sum(x => x.amount);
        }

        public static decimal NonShippingAdjustment(this Refund refund)
        {
            return refund.order_adjustments
                        .Where(x => !x.IsShippingAdjustment())
                        .Sum(x => x.amount);
        }

    }
}
