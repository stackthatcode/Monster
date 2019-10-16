using System;
using Hangfire;
using Monster.Middle.Persist.Instance;
using Monster.Middle.Processes.Sync.Services;


namespace Monster.Middle.Hangfire
{
    public class RecurringJobService
    {
        private readonly InstanceContext _instanceContext;
        private readonly ExecutionLogService _executionLogService;
        private readonly ExclusiveJobMonitoringService _jobMonitoringService;


        public RecurringJobService(
                InstanceContext instanceContext, 
                ExecutionLogService executionLogService,
                ExclusiveJobMonitoringService jobMonitoringService)
        {
            _instanceContext = instanceContext;
            _executionLogService = executionLogService;
            _jobMonitoringService = jobMonitoringService;
        }

        public void StartEndToEndSync()
        {
            var jobId = _jobMonitoringService.RecurringEndToEndSyncJobId;

            using (var transaction = _jobMonitoringService.BeginTransaction())
            {
                RecurringJob.AddOrUpdate<ExclusiveJobRunner>(
                    jobId,
                    x => x.EndToEndSync(_instanceContext.InstanceId),
                    "*/1 * * * *",
                    TimeZoneInfo.Utc);

                _jobMonitoringService.AddJobMonitor(
                        ExclusiveJobType.EndToEndSync, jobId);

                _executionLogService.InsertExecutionLog("End-to-End Sync - Starting Background Job");

                RecurringJob.Trigger(jobId);
                transaction.Commit();
            }
        }

        public void KillEndToEndSync()
        {
            using (var transaction = _jobMonitoringService.BeginTransaction())
            {
                var hangfireJobId = _jobMonitoringService.RecurringEndToEndSyncJobId;
                RecurringJob.RemoveIfExists(hangfireJobId);

                _jobMonitoringService.RemoveRecurringJobMonitor(_jobMonitoringService.RecurringEndToEndSyncJobId);

                _executionLogService.InsertExecutionLog("End-to-End Sync - Killing Background Job");
                transaction.Commit();
            }
        }
    }
}

