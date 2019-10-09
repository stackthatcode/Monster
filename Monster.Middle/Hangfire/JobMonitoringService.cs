using System;
using System.Data.Entity;
using System.Linq;
using Hangfire;
using Monster.Middle.Persist.Instance;

namespace Monster.Middle.Hangfire
{
    public class JobMonitoringService
    {
        private readonly InstanceContext _instanceContext;
        private readonly InstancePersistContext _dataContext;
        private MonsterDataContext Entities => _dataContext.Entities;
        
        // Always use the same Job ID for Recurring Jobs
        //
        public string RecurringEndToEndSyncJobId => "RecurringEndToEndSync:" + _instanceContext.InstanceId;


        public JobMonitoringService(InstanceContext instanceContext, InstancePersistContext dataContext)
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
            return GetMonitoringDigest().IsJobTypeActive(BackgroundJobType.EndToEndSync);
        }

        public InstanceMonitorDigest GetMonitoringDigest()
        {
            var jobRecords = Entities.JobMonitors.AsNoTracking().ToList();
            var output = new InstanceMonitorDigest();

            foreach (var jobRecord in jobRecords)
            {
                var model = BuildJobMonitorModel(jobRecord);
                output.JobMonitors.Add(model);
            }

            return output;
        }

        private JobMonitorDigest BuildJobMonitorModel(JobMonitor sqlRecord)
        {
            var output = new JobMonitorDigest();

            output.BackgroundJobType = sqlRecord.BackgroundJobType;
            output.HangfireJobId = sqlRecord.HangFireJobId;
            output.IsRecurringJob = sqlRecord.IsRecurring;

            if (sqlRecord.IsRecurring)
            {
                output.IsJobActive = true;
            }
            else
            {
                using (var connection = JobStorage.Current.GetConnection())
                {
                    var hangfireRecord = connection.GetJobData(sqlRecord.HangFireJobId);
                    if (hangfireRecord == null)
                    {
                        output.IsJobActive = false;
                    }
                    else
                    {
                        output.IsJobActive = hangfireRecord.IsRunning();
                    }
                }
            }

            return output;
        }

        // Address edge-case wherein Hangfire Job may exist, but Job Monitor record does not
        //
        public void CleanupOrphanedRecurring()
        {
            var monitor = Entities.JobMonitors.FirstOrDefault(
                    x => x.HangFireJobId == this.RecurringEndToEndSyncJobId);
            
            // Cannot locate Monitor, therefore remove from Hangfire
            if (monitor == null)
            {
                RecurringJob.RemoveIfExists(this.RecurringEndToEndSyncJobId);
            }
        }


        public JobMonitor AddJobMonitor(int backgroundJobType, string hangFireJobId, bool isRecurring)
        {
            RemoveJobMonitors(backgroundJobType);

            var newJob = new JobMonitor()
            {
                BackgroundJobType = backgroundJobType,
                HangFireJobId = hangFireJobId,
                IsRecurring = isRecurring,
                DateCreated = DateTime.UtcNow,
                LastUpdated = DateTime.UtcNow,
            };

            Entities.JobMonitors.Add(newJob);
            Entities.SaveChanges();

            return newJob;
        }

        public void RemoveJobMonitors(int backgroundJobType)
        {
            var monitors = Entities.JobMonitors.Where(x => x.BackgroundJobType == backgroundJobType).ToList();

            foreach (var monitor in monitors)
            {
                Entities.JobMonitors.Remove(monitor);
            }

            Entities.SaveChanges();
        }

        public void RemoveRecurringJobMonitor(string hangfireJobId)
        {
            var entity = Entities
                    .JobMonitors
                    .FirstOrDefault(x => x.IsRecurring == true && x.HangFireJobId == hangfireJobId);

            Entities.JobMonitors.Remove(entity);
            Entities.SaveChanges();
        }

        public void RemoveOneTimeJobMonitor(int backgroundJobType)
        {
            var jobs = Entities
                .JobMonitors
                .Where(x => x.BackgroundJobType == backgroundJobType && x.IsRecurring == false)
                .ToList();

            foreach (var job in jobs)
            {
                Entities.JobMonitors.Remove(job);
            }
            Entities.SaveChanges();
        }
    }
}
