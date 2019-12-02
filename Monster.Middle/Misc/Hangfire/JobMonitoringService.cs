using System;
using System.Data.Entity;
using System.Linq;
using Hangfire;
using Monster.Middle.Persist.Instance;
using Push.Foundation.Utilities.Helpers;

namespace Monster.Middle.Misc.Hangfire
{
    public class JobMonitoringService
    {
        private readonly InstanceContext _instanceContext;
        private readonly MiscPersistContext _dataContext;
        private MonsterDataContext Entities => _dataContext.Entities;
        
        // Always use the same Job ID for Recurring Jobs
        //
        public string RecurringEndToEndSyncJobId => "RecurringEndToEndSync:" + _instanceContext.InstanceId;


        public JobMonitoringService(InstanceContext instanceContext, MiscPersistContext dataContext)
        {
            _instanceContext = instanceContext;
            _dataContext = dataContext;
        }

        public DbContextTransaction BeginTransaction()
        {
            return Entities.Database.BeginTransaction();
        }

        private static readonly object _provisioningLock = new object();

        public ExclusiveJobMonitor ProvisionJobMonitor(
                int backgroundJobType, bool isRecurring, string hangFireJobId = null)
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

        public bool IsInterruptedByJobType(int jobType)
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

        public void SendKillSignal(long monitorId)
        {
            var monitor = Entities
                .ExclusiveJobMonitors
                .FirstOrDefault(x => x.Id == monitorId);

            if (monitor == null)
            {
                return;
            }

            monitor.ReceivedKillSignal = true;
            Entities.SaveChanges();
        }

        public void RemoveJobMonitor(long monitorId)
        {
            var monitor = Entities.ExclusiveJobMonitors.FirstOrDefault(x => x.Id == monitorId);
            Entities.ExclusiveJobMonitors.Remove(monitor);
            Entities.SaveChanges();
        }

        public void CleanupPostExecution(long monitorId)
        {
            // First explicitly remove the Job Monitor
            //
            var monitor = Entities.ExclusiveJobMonitors.FirstOrDefault(x => x.Id == monitorId);
            if (monitor != null)
            {
                if (monitor.IsRecurring && monitor.ReceivedKillSignal)
                {
                    RecurringJob.RemoveIfExists(monitor.HangFireJobId);
                    RemoveJobMonitor(monitor.Id);
                }

                if (!monitor.IsRecurring)
                {
                    RemoveJobMonitor(monitor.Id);
                }
            }

            Cleanup();
        }


        public void Cleanup()
        {
            // Remove the Recurring Job if there's no Job Monitor
            //
            if (!Entities.ExclusiveJobMonitors.Any(x => x.HangFireJobId == RecurringEndToEndSyncJobId))
            {
                RecurringJob.RemoveIfExists(RecurringEndToEndSyncJobId);
            }

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
                    if (!monitor.IsRecurring)
                    {
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
}

