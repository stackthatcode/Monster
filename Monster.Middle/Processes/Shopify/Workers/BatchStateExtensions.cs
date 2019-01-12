using System;

namespace Monster.Middle.Processes.Shopify.Workers
{
    public static class BatchStateExtensions
    {
        public const int PullEndFudgeMinutes = -15;

        public static DateTime AddBatchFudge(this DateTime input)
        {
            return input.AddMinutes(PullEndFudgeMinutes);
        }

    }
}
