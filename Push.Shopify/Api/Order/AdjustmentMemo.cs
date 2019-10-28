using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Push.Shopify.Api.Order
{
    public enum AdjustmentMemoType
    {
        CreditMemo = 1,
        DebitMemo = 2,
    }

    public class AdjustmentMemo
    {
        public AdjustmentMemoType Type { get; set; }
        public decimal Amount { get; set; }
        public Refund SourceRefund { get; set; }
    }

    public static class AdjustmentMemoExtensions
    {
        public static List<AdjustmentMemo> GetAdjustmentMemos(this Order order)
        {
            var output = new List<AdjustmentMemo>();

            foreach (var refund in order.refunds)
            {
                if (refund.CreditMemoTotal > 0)
                {
                    output.Add(new AdjustmentMemo
                    {
                        Type = AdjustmentMemoType.CreditMemo,
                        Amount = refund.CreditMemoTotal,
                        SourceRefund = refund
                    });

                    continue;
                }

                if (refund.DebitMemoTotal > 0)
                {
                    output.Add(new AdjustmentMemo
                    {
                        Type = AdjustmentMemoType.DebitMemo,
                        Amount = refund.DebitMemoTotal,
                        SourceRefund = refund
                    });

                    continue;
                }

            }

            return output;
        }
    }
}

