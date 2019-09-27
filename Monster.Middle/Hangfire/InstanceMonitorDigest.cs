using System.Collections.Generic;
using System.Linq;

namespace Monster.Middle.Hangfire
{
    public class InstanceMonitorDigest
    {
        public List<JobMonitorDigest> JobMonitors { get; set; }
        public bool AreAnyJobsActive => JobMonitors.Any(x => x.IsJobActive);

        public bool IsJobTypeActive(int backgroundJobType)
        {
            return JobMonitors.Any(
                x => x.BackgroundJobType == backgroundJobType && x.IsJobActive == true);
        }

        public InstanceMonitorDigest()
        {
            JobMonitors = new List<JobMonitorDigest>();
        }
    }

    public class JobMonitorDigest
    {
        public int BackgroundJobType { get; set; }
        public string HangfireJobId { get; set; }
        public bool IsRecurringJob { get; set; }
        public bool IsJobActive { get; set; }
    }
}

