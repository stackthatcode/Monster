using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Hangfire;
using Monster.Middle.Persist.Multitenant;
using Monster.Middle.Processes.Sync.Inventory.Model;
using Monster.Middle.Security;
using Push.Foundation.Utilities.Helpers;
using Push.Foundation.Utilities.Logging;


namespace Monster.Middle.Hangfire
{
    public class OneTimeJobService
    {
        private readonly ConnectionContext _tenantContext;
        private readonly ConnectionRepository _connectionRepository;
        private readonly StateRepository _stateRepository;
        private readonly IPushLogger _logger;


        public OneTimeJobService(
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


        public void ConnectToAcumatica()
        {
            QueueJob(BackgroundJobType.ConnectToAcumatica,
                x => x.RunConnectToAcumatica(_tenantContext.InstanceId));
        }

        public void PullAcumaticaRefData()
        {
            QueueJob(BackgroundJobType.PullAcumaticaRefData, 
                x => x.RunPullAcumaticaRefData(_tenantContext.InstanceId));
        }

        public void SyncWarehouseAndLocation()
        {
            QueueJob(BackgroundJobType.SyncWarehouseAndLocation,
                x => x.RunSyncWarehouseAndLocation(_tenantContext.InstanceId));
        }
    
        public void RunDiagnostics()
        {
            QueueJob(BackgroundJobType.Diagnostics, x => x.RunDiagnostics(_tenantContext.InstanceId));
        }

        public void PullInventory()
        {
            QueueExclusiveJob(
                BackgroundJobType.PullInventory, x => x.PullInventory(_tenantContext.InstanceId));
        }

        public void ImportIntoAcumatica(
                List<long> spids, bool createInventoryReceipts, bool automaticEnable)
        {
            var context = new AcumaticaInventoryImportContext
            {
                ShopifyProductIds = spids,
                CreateInventoryReceipts = createInventoryReceipts,
                IsSyncEnabled = automaticEnable,
            };

            QueueExclusiveJob(
                BackgroundJobType.ImportIntoAcumatica,
                x => x.ImportIntoAcumatica(_tenantContext.InstanceId, context));
        }
        

        // Worker methods
        //
        private void QueueJob(int jobType, Expression<Action<JobRunner>> action)
        {
            using (var transaction = _stateRepository.BeginTransaction())
            {
                var jobId = BackgroundJob.Enqueue<JobRunner>(action);
                _stateRepository.InsertBackgroundJob(jobType, jobId);                
                transaction.Commit();
            }
        }

        private void QueueExclusiveJob(
                    int jobType, Expression<Action<ExclusiveJobRunner>> action)
        {
            using (var transaction = _stateRepository.BeginTransaction())
            {
                var jobId = BackgroundJob.Enqueue<ExclusiveJobRunner>(action);
                _stateRepository.InsertBackgroundJob(jobType, jobId);
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

                RecurringJob.AddOrUpdate<ExclusiveJobRunner>(  
                    routineSyncJobId,
                    x => x.RealTimeSync(_tenantContext.InstanceId),
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
        public bool AreAnyJobsRunning(List<int> jobTypes)
        {
            return jobTypes.Any(x => IsJobRunning(x));
        }

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
        
    }
}

