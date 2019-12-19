using System;
using System.Linq;
using Hangfire;
using Monster.Middle.Misc.Logging;
using Monster.Middle.Persist.Instance;

namespace Monster.Middle.Misc.Hangfire
{
    public class RecurringJobScheduler
    {
        private readonly InstanceContext _instanceContext;
        private readonly ExecutionLogService _executionLogService;
        private readonly JobMonitoringService _jobMonitoringService;
        private readonly OneTimeJobScheduler _oneTimeJobScheduler;


        // Always use the same Job ID for Recurring Jobs
        //
        public string RecurringEndToEndSyncJobId => "RecurringEndToEndSync:" + _instanceContext.InstanceId;


        public RecurringJobScheduler(
                InstanceContext instanceContext, 
                ExecutionLogService executionLogService,
                JobMonitoringService jobMonitoringService, 
                OneTimeJobScheduler oneTimeJobScheduler)
        {
            _instanceContext = instanceContext;
            _executionLogService = executionLogService;
            _jobMonitoringService = jobMonitoringService;
            _oneTimeJobScheduler = oneTimeJobScheduler;
        }

        public void StartEndToEndSync()
        {
            RecurringJob.AddOrUpdate<JobRunner>(
                RecurringEndToEndSyncJobId, 
                x => x.TriggerEndToEndSync(_instanceContext.InstanceId),
                "0 * * * *", 
                TimeZoneInfo.Utc);

            _executionLogService.Log("End-to-End Sync - Recurring Job Started");
        }

        public bool IsEndToEndSyncActive()
        {
            using (var connection = JobStorage.Current.GetConnection())
            {
                var entries = connection.GetAllEntriesFromHash("recurring-job:" + RecurringEndToEndSyncJobId);
                if (entries == null)
                {
                    return false;
                }
                else
                {
                    return entries.Any();
                }
            }
        }

        public void KillEndToEndSync()
        {
            RecurringJob.RemoveIfExists(RecurringEndToEndSyncJobId);

            _executionLogService.Log("End-to-End Sync - Recurring Job Paused");
        }
    }
}

