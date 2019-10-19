using System;

namespace Monster.Middle.Processes.Shopify.Persist
{
    public static class BatchStateExtensions
    {
        public const int GetEndFudgeMinutes = -5;

        public static DateTime SubtractFudgeFactor(this DateTime input)
        {
            return input.AddMinutes(GetEndFudgeMinutes);
        }
    }
}
