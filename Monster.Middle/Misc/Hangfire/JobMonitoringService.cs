using System;
using System.Data.Entity;
using System.Linq;
using Hangfire;
using Monster.Middle.Misc.Logging;
using Monster.Middle.Persist.Instance;
using Push.Foundation.Utilities.Helpers;

namespace Monster.Middle.Misc.Hangfire
{
    public class JobMonitoringService
    {
        private readonly InstanceContext _instanceContext;
        private readonly MiscPersistContext _dataContext;
        private readonly ExecutionLogService _executionLogService;

        private MonsterDataContext Entities => _dataContext.Entities;
        

        public JobMonitoringService(
                InstanceContext instanceContext, 
                MiscPersistContext dataContext, 
                ExecutionLogService executionLogService)
        {
            _instanceContext = instanceContext;
            _dataContext = dataContext;
            _executionLogService = executionLogService;
        }

        public DbContextTransaction BeginTransaction()
        {
            return Entities.Database.BeginTransaction();
        }

        private static readonly object _provisioningLock = new object();

        public ExclusiveJobMonitor ProvisionMonitor(int backgroundJobType, string hangFireJobId = null)
        {
            lock (_provisioningLock)
            {
                Cleanup();

                using (var transaction = BeginTransaction())
                {
                    if (AreAnyJobsRunning())
                    {
                        return null;
                    }

                    var newJob = new ExclusiveJobMonitor()
                    {
                        BackgroundJobType = backgroundJobType,
                        HangFireJobId = hangFireJobId,
                        ReceivedKillSignal = false,
                        DateCreated = DateTime.UtcNow,
                        LastUpdated = DateTime.UtcNow,
                    };

                    Entities.ExclusiveJobMonitors.Add(newJob);
                    Entities.SaveChanges();

                    transaction.Commit();
                    return newJob;
                }
            }
        }

        public bool IsJobRunning(int backgroundJobType)
        {
            return Entities.ExclusiveJobMonitors
                .AsNoTracking()
                .Any(x => x.BackgroundJobType == backgroundJobType);
        }

        public bool AreAnyJobsRunning()
        {
            return Entities.ExclusiveJobMonitors
                .AsNoTracking()
                .Any();
        }

        public void AssignHangfireJob(long monitorId, string hangfireJobId)
        {
            var monitor = Entities.ExclusiveJobMonitors.FirstOrDefault(x => x.Id == monitorId);
            if (monitor != null)
            {
                monitor.HangFireJobId = hangfireJobId;
                Entities.SaveChanges();
            }
        }

        public ExclusiveJobMonitor RetrieveMonitorNoTracking(long monitorId)
        {
            return Entities
                    .ExclusiveJobMonitors
                    .AsNoTracking()
                    .FirstOrDefault(x => x.Id == monitorId);
        }

        public ExclusiveJobMonitor RetrieveMonitorByTypeNoTracking(int jobType)
        {
            return Entities
                .ExclusiveJobMonitors
                .AsNoTracking()
                .FirstOrDefault(x => x.BackgroundJobType == jobType);
        }

        public bool IsJobTypeInterrupted(int jobType)
        {
            var monitor = RetrieveMonitorByTypeNoTracking(jobType);
            return (monitor == null || monitor.ReceivedKillSignal);
        }

        public bool IsInterrupted(long monitorId)
        {
            var monitor = RetrieveMonitorNoTracking(monitorId);
            return (monitor == null || monitor.ReceivedKillSignal);
        }

        public bool IsCorrupted(long monitorId)
        {
            var monitor = RetrieveMonitorNoTracking(monitorId);
            return monitor != null && monitor.HangFireJobId.IsNullOrEmpty();
        }

        public void SendKillSignal()
        {
            foreach (var monitor in Entities.ExclusiveJobMonitors.ToList())
            {
                SendKillSignal(monitor.Id);
            }
        }

        public void SendKillSignal(long monitorId)
        {
            var monitor = Entities.ExclusiveJobMonitors.FirstOrDefault(x => x.Id == monitorId);
            if (monitor == null)
            {
                return;
            }

            var description = BackgroundJobType.Name[monitor.BackgroundJobType];
            _executionLogService.Log($"{description} - Interrupt Signal received");

            monitor.ReceivedKillSignal = true;
            Entities.SaveChanges();
        }

        public void RemoveJobMonitor(long monitorId)
        {
            var monitor = Entities.ExclusiveJobMonitors.FirstOrDefault(x => x.Id == monitorId);
            Entities.ExclusiveJobMonitors.Remove(monitor);
            Entities.SaveChanges();
        }

        public void CleanupPostExecution(long finishedJobMonitor)
        {
            // First explicitly remove the Job Monitor
            //
            var monitor = Entities.ExclusiveJobMonitors.FirstOrDefault(x => x.Id == finishedJobMonitor);
            if (monitor != null)
            {
                RemoveJobMonitor(monitor.Id);
            }

            Cleanup();
        }


        public void Cleanup()
        {
            var monitors = Entities.ExclusiveJobMonitors.ToList();

            using (var connection = JobStorage.Current.GetConnection())
            {
                foreach (var monitor in monitors)
                {
                    // Remove any Job Monitor that is corrupted -- or doesn't have a valid Hangfire Job
                    //
                    if (monitor.HangFireJobId.IsNullOrEmpty())
                    {
                        RemoveJobMonitor(monitor.Id);
                        continue;
                    }

                    // Remove any Job Monitor for a One-Time Job that is no longer Alive
                    //
                    var hangfireRecord = connection.GetJobData(monitor.HangFireJobId);
                    if (!hangfireRecord.IsAlive())
                    {
                        RemoveJobMonitor(monitor.Id);
                    }
                }
            }
        }
    }
}

