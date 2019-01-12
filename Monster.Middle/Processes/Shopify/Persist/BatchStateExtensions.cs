using System;

namespace Monster.Middle.Processes.Shopify.Persist
{
    public static class BatchStateExtensions
    {
        public const int PullEndFudgeMinutes = -10;

        public static DateTime AddShopifyBatchFudge(this DateTime input)
        {
            return input.AddMinutes(PullEndFudgeMinutes);
        }

    }
}
