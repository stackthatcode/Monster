using Hangfire.Storage;

namespace Monster.Middle.Misc.Hangfire
{
    public static class HangfireExtensions
    {
        public static bool IsAlive(this string jobState)
        {
            if (jobState == "Succeeded" || jobState == "Failed" || jobState == "Deleted")
            {
                return false;
            }

            return true;
        }

        public static bool IsAlive(this JobData jobData)
        {
            if (jobData == null)
            {
                return false;
            }
            else
            {
                return jobData.State.IsAlive();
            }
        }
    }
}

