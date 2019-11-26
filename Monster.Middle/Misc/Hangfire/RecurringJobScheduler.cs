using System;
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


        public RecurringJobScheduler(
                InstanceContext instanceContext, 
                ExecutionLogService executionLogService,
                JobMonitoringService jobMonitoringService)
        {
            _instanceContext = instanceContext;
            _executionLogService = executionLogService;
            _jobMonitoringService = jobMonitoringService;
        }

        public void StartEndToEndSync()
        {
            var jobId = _jobMonitoringService.RecurringEndToEndSyncJobId;

            var monitor = _jobMonitoringService
                .ProvisionJobMonitor(BackgroundJobType.EndToEndSync, true, jobId);

            RecurringJob.AddOrUpdate<JobRunner>(
                jobId,
                x => x.EndToEndSync(_instanceContext.InstanceId, monitor.Id),
                "*/1 * * * *",
                TimeZoneInfo.Utc);

            _jobMonitoringService.AssignHangfireJob(monitor.Id, jobId);

            _executionLogService.Log("End-to-End Sync - recurring job - scheduled");

            RecurringJob.Trigger(jobId);
        }
        
        public void KillEndToEndSync()
        {
            var monitor = _jobMonitoringService.RetrieveMonitorByTypeNoTracking(BackgroundJobType.EndToEndSync);
            if (monitor != null)
            {
                _jobMonitoringService.SendKillSignal(monitor.Id);
                _executionLogService.Log($"End-to-End Sync - recurring job - stop signal received");
            }
        }
    }
}

