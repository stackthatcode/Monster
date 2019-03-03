using Hangfire.Storage;

namespace Monster.Middle.Hangfire
{
    public static class Extensions
    {
        public static bool IsRunning(this string jobState)
        {
            if (jobState == "Succeeded" || jobState == "Failed" || jobState == "Deleted")
            {
                return false;
            }

            return true;
        }

        public static bool IsRunning(this JobData jobData)
        {
            if (jobData == null)
            {
                return false;
            }
            else
            {
                return jobData.State.IsRunning();
            }
        }

        public static bool IsRunning(this StateData jobData)
        {
            if (jobData == null)
            {
                return false;
            }
            else
            {
                return jobData.Name.IsRunning();
            }
        }
    }
}

