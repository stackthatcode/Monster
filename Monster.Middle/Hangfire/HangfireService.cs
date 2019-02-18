﻿using System;
using System.Linq;
using System.Linq.Expressions;
using Hangfire;
using Hangfire.Storage;
using Monster.Middle.Persist.Multitenant;
using Monster.Middle.Security;
using Push.Foundation.Utilities.Helpers;
using Push.Foundation.Utilities.Logging;


namespace Monster.Middle.Hangfire
{
    public class HangfireService
    {
        private readonly ConnectionContext _tenantContext;
        private readonly ConnectionRepository _connectionRepository;
        private readonly StateRepository _stateRepository;
        private readonly IPushLogger _logger;


        public HangfireService(
                IPushLogger logger,
                ConnectionContext tenantContext, 
                ConnectionRepository connectionRepository,
                StateRepository stateRepository)
        {
            _tenantContext = tenantContext;
            _connectionRepository = connectionRepository;
            _logger = logger;
            _stateRepository = stateRepository;
        }


        public void LaunchJob(int jobId)
        {
            if (jobId == JobType.ConnectToAcumatica)
            {
                QueueBackgroundJob(
                    JobType.ConnectToAcumatica,
                    x => x.RunConnectToAcumatica(_tenantContext.InstanceId));
                return;
            }

            if (jobId == JobType.PullAcumaticaRefData)
            {
                QueueBackgroundJob(
                    JobType.PullAcumaticaRefData,
                    x => x.RunPullAcumaticaRefData(_tenantContext.InstanceId));
                return;
            }

            if (jobId == JobType.SyncWarehouseAndLocation)
            {
                QueueBackgroundJob(
                    JobType.SyncWarehouseAndLocation,
                    x => x.RunSyncWarehouseAndLocation(_tenantContext.InstanceId));
                return;
            }

            if (jobId == JobType.Diagnostics)
            {
                QueueBackgroundJob(
                    JobType.Diagnostics,
                    x => x.RunDiagnostics(_tenantContext.InstanceId));
                return;
            }

            if (jobId == JobType.PushInventoryToAcumatica)
            {
                QueueBackgroundJob(
                    JobType.PushInventoryToAcumatica,
                    x => x.PushInventoryToAcumatica(_tenantContext.InstanceId));
                return;
            }

            if (jobId == JobType.PushInventoryToShopify)
            {
                QueueBackgroundJob(
                    JobType.PushInventoryToShopify,
                    x => x.PushInventoryToShopify(_tenantContext.InstanceId));
                return;
            }

            throw new ArgumentException($"Unrecognized jobId {jobId}");
        }
        
        private void QueueBackgroundJob(
                int backgroundJobType, Expression<Action<JobRunner>> action)
        {
            using (var transaction = _stateRepository.BeginTransaction())
            {
                // This is no longer necessary since execution will be blocked by the Job Runner
                //if (IsJobRunning(backgroundJobType))
                //{
                //    _logger.Info($"Job (BackgroundJobType = {backgroundJobType}) already running");
                //    return;
                //}
                
                var jobId = BackgroundJob.Enqueue<JobRunner>(action);
                _stateRepository.InsertBackgroundJob(backgroundJobType, jobId);                
                transaction.Commit();
            }
        }



        // Recurring Job - separate category
        //
        public void StartRoutineSync()
        {
            var routineSyncJobId = RoutineSyncJobId();            
            using (var transaction = _stateRepository.BeginTransaction())
            {
                var state = _stateRepository.RetrieveSystemState();

                RecurringJob.AddOrUpdate<JobRunner>(  
                    routineSyncJobId,
                    x => x.RealTimeSynchronization(_tenantContext.InstanceId),
                    "*/1 * * * *",
                    TimeZoneInfo.Utc);

                state.RealTimeHangFireJobId = routineSyncJobId;;
                _connectionRepository.Entities.SaveChanges();

                RecurringJob.Trigger(routineSyncJobId);
                transaction.Commit();
            }
        }

        public void PauseRoutineSync()
        {
            using (var transaction = _connectionRepository.BeginTransaction())
            {
                var state = _stateRepository.RetrieveSystemState();
                
                var jobId = state.RealTimeHangFireJobId;
                if (jobId.IsNullOrEmpty())
                {
                    return;
                }

                RecurringJob.RemoveIfExists(jobId);
                state.RealTimeHangFireJobId = null;
                _stateRepository.Entities.SaveChanges();

                transaction.Commit();
            }
        }

        private string RoutineSyncJobId()
        {
            return "RoutineSync:" + _tenantContext.InstanceId;
        }

        
        // Status querying
        //
        public bool IsJobRunning(int jobType)
        {
            // If Background Job Record is missing, return false
            var jobRecord = _stateRepository.RetrieveBackgroundJob(jobType);
            if (jobRecord == null)
            {
                return false;
            }
            else
            {
                return IsHangFireFafJobRunning(jobRecord.HangFireJobId);
            }
        }

        public bool IsRealTimeSyncRunning()
        {
            var state = _stateRepository.RetrieveSystemState();
            return !state.RealTimeHangFireJobId.IsNullOrEmpty();
        }


        // Hangfire specific methods
        //
        public bool IsHangFireFafJobRunning(string hangfireJobId)
        {
            using (var connection = JobStorage.Current.GetConnection())
            {
                var jobData = connection.GetJobData(hangfireJobId);
                return jobData.IsRunning();
            }
        }

        [Obsolete("Unsure if this actually provides the correct answer")]
        public bool IsHangfireRecurringJobRunning(string hangFireJobId)
        {
            using (var connection = JobStorage.Current.GetConnection())
            {
                var recurringJobs = connection.GetRecurringJobs();
                var job = recurringJobs.FirstOrDefault(p => p.Id == hangFireJobId);

                var jobState = connection.GetStateData(job.LastJobId);
                return jobState.IsRunning();
            }
        }
        
    }
}

