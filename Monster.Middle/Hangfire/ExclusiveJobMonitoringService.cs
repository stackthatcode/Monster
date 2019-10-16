using System;
using System.Data.Entity;
using System.Linq;
using Hangfire;
using Hangfire.Storage;
using Monster.Middle.Persist.Instance;

namespace Monster.Middle.Hangfire
{
    public class ExclusiveJobMonitoringService
    {
        private readonly InstanceContext _instanceContext;
        private readonly InstancePersistContext _dataContext;
        private MonsterDataContext Entities => _dataContext.Entities;
        
        // Always use the same Job ID for Recurring Jobs
        //
        public string RecurringEndToEndSyncJobId => "RecurringEndToEndSync:" + _instanceContext.InstanceId;


        public ExclusiveJobMonitoringService(InstanceContext instanceContext, InstancePersistContext dataContext)
        {
            _instanceContext = instanceContext;
            _dataContext = dataContext;
        }

        public DbContextTransaction BeginTransaction()
        {
            return Entities.Database.BeginTransaction();
        }

        public bool IsEndToEndSyncRunning()
        {
            return GetMonitoringDigest().IsJobTypeActive(ExclusiveJobType.EndToEndSync);
        }

        public InstanceMonitorDigest GetMonitoringDigest()
        {
            var jobRecords = Entities.ExclusiveJobMonitors.AsNoTracking().ToList();
            var output = new InstanceMonitorDigest();

            using (var connection = JobStorage.Current.GetConnection())
            {
                foreach (var jobRecord in jobRecords)
                {
                    var model = BuildJobMonitorDigest(connection, jobRecord);
                    output.JobMonitors.Add(model);
                }
            }

            return output;
        }

        private JobMonitorDigest 
                BuildJobMonitorDigest(IStorageConnection connection, ExclusiveJobMonitor monitor)
        {
            var output = new JobMonitorDigest();

            output.BackgroundJobType = monitor.BackgroundJobType;
            output.HangfireJobId = monitor.HangFireJobId;

            if (monitor.BackgroundJobType.IsRecurring())
            {
                var recurringJobExists
                    = connection
                        .GetRecurringJobs()
                        .Any(x => x.Id == monitor.HangFireJobId);

                output.IsJobActive = recurringJobExists;
            }
            else
            {
                var hangfireRecord = connection.GetJobData(monitor.HangFireJobId);
                if (hangfireRecord == null)
                {
                    output.IsJobActive = false;
                }
                else
                {
                    output.IsJobActive = hangfireRecord.IsRunning();
                }
            }

            return output;
        }


        // Address edge-case wherein Hangfire Job may exist, but Job Monitor record does not
        //
        public void CleanupOrphanedRecurring()
        {
            var monitor = Entities.ExclusiveJobMonitors.FirstOrDefault(
                    x => x.HangFireJobId == this.RecurringEndToEndSyncJobId);
            
            // Cannot locate Monitor, therefore remove from Hangfire
            if (monitor == null)
            {
                RecurringJob.RemoveIfExists(this.RecurringEndToEndSyncJobId);
            }
        }


        public ExclusiveJobMonitor AddJobMonitor(int backgroundJobType, string hangFireJobId)
        {
            var newJob = new ExclusiveJobMonitor()
            {
                BackgroundJobType = backgroundJobType,
                HangFireJobId = hangFireJobId,
                IsMarkedForDeath = false,
                DateCreated = DateTime.UtcNow,
                LastUpdated = DateTime.UtcNow,
            };

            Entities.ExclusiveJobMonitors.Add(newJob);
            Entities.SaveChanges();

            return newJob;
        }

        public ExclusiveJobMonitor RetrieveMonitorNoTracking(long monitorId)
        {
            return Entities.ExclusiveJobMonitors
                    .AsNoTracking()
                    .FirstOrDefault(x => x.Id == monitorId);
        }

        public bool IsMissingOrMarkedForDeath(long monitorId)
        {
            var monitor = RetrieveMonitorNoTracking(monitorId);
            return (monitor == null || monitor.IsMarkedForDeath);
        }

        public void KillHangfireJob(long monitorId)
        {
            var monitor = RetrieveMonitorNoTracking(monitorId);
            if (monitor == null)
            {
                return;
            }

            if (monitor.BackgroundJobType.IsRecurring())
            {
                RecurringJob.RemoveIfExists(monitor.HangFireJobId);
            }
            else
            {
                BackgroundJob.Delete(monitor.HangFireJobId);
            }
        }

        public void RemoveJobMonitors(int backgroundJobType)
        {
            var monitors 
                = Entities
                    .ExclusiveJobMonitors
                    .Where(x => x.BackgroundJobType == backgroundJobType).ToList();

            foreach (var monitor in monitors)
            {
                Entities.ExclusiveJobMonitors.Remove(monitor);
            }

            Entities.SaveChanges();
        }

        public void RemoveJobMonitor(int monitorId)
        {
            var job = Entities.ExclusiveJobMonitors.FirstOrDefault(x => x.Id == monitorId);

            if (job != null)
            {
                Entities.ExclusiveJobMonitors.Remove(job);
                Entities.SaveChanges();
            }
        }
    }
}
