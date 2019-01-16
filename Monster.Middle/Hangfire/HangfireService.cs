using System;
using System.Linq.Expressions;
using Hangfire;
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


        static readonly object LockConnectToAcumatica = new object();
        static readonly object LockPullAcumaticaReferenceData = new object();
        static readonly object LockSyncWarehouseAndLocation = new object();
        static readonly object LockPushInventoryToAcumatica = new object();
        static readonly object LockPushInventoryToShopify = new object();
        static readonly object LockConfigDiagnosis = new object();


        public bool IsJobRunning(int backgroundJobId)
        {
            // If Background Job Record is missing, return false
            var jobRecord = _stateRepository.RetrieveBackgroundJob(backgroundJobId);
            if (jobRecord == null)
            {
                return false;
            }

            // This should hit the System Database
            var hangfireJobId = jobRecord.HangFireJobId;
            var connection = JobStorage.Current.GetConnection();
            var jobData = connection.GetJobData(hangfireJobId);

            // If Hangfire Job is missing, return false
            if (jobData == null)
            {
                return false;
            }

            if (jobData.State == "Succeeded" || jobData.State == "Failed" ||
                jobData.State == "Deleted")
            {
                return false;
            }

            return true;
        }

        public void ConnectToAcumatica()
        {            
            var tenantId = _tenantContext.InstanceId;

            SafelyEnqueueBackgroundJob(
                LockConnectToAcumatica,
                BackgroundJobType.ConnectToAcumatica,
                x => x.RunConnectToAcumatica(tenantId));
        }

        public void PullAcumaticaReferenceData()
        {
            var tenantId = _tenantContext.InstanceId;

            SafelyEnqueueBackgroundJob(
                LockPullAcumaticaReferenceData,
                BackgroundJobType.PullAcumaticaReferenceData,
                x => x.RunPullAcumaticaSettings(tenantId));
        }

        public void SyncWarehouseAndLocation()
        {
            var tenantId = _tenantContext.InstanceId;

            SafelyEnqueueBackgroundJob(
                LockSyncWarehouseAndLocation,
                BackgroundJobType.SyncWarehouseAndLocation,
                x => x.RunSyncWarehouseAndLocation(tenantId));
        }
        
        public void PushInventoryToAcumatica()
        {
            var tenantId = _tenantContext.InstanceId;

            SafelyEnqueueBackgroundJob(
                LockPushInventoryToAcumatica,
                BackgroundJobType.PushInventoryToAcumatica,
                x => x.RunLoadInventoryIntoAcumatica(tenantId));
        }
        
        public void PushInventoryToShopify()
        {
            var tenantId = _tenantContext.InstanceId;

            SafelyEnqueueBackgroundJob(
                LockPushInventoryToShopify,
                BackgroundJobType.PushInventoryToShopify,
                x => x.RunLoadInventoryIntoShopify(tenantId));
        }
        
        public void TriggerConfigDiagnosis()
        {
            var tenantId = _tenantContext.InstanceId;

            SafelyEnqueueBackgroundJob(
                LockConfigDiagnosis,
                BackgroundJobType.Diagnostics,
                x => x.RunDiagnostics(tenantId));

        }

        private void SafelyEnqueueBackgroundJob(
                object lockObject,
                int backgroundJobType, 
                Expression<Action<BackgroundJobRunner>> action)
        {
            lock (lockObject)
            {
                using (var transaction = _stateRepository.BeginTransaction())
                {
                    if (IsJobRunning(backgroundJobType))
                    {
                        _logger.Info($"Job (BackgroundJobType = {backgroundJobType}) already running");
                        return;
                    }
                    
                    var jobId = BackgroundJob.Enqueue<BackgroundJobRunner>(action);

                    _stateRepository.InsertBackgroundJob(backgroundJobType, jobId);
                    
                    transaction.Commit();
                }
            }
        }
        



        // Recurring Job - separate category
        public string RoutineSyncJobId()
        {
            return "RoutineSync:" + _tenantContext.InstanceId;
        }

        public void StartRoutineSync()
        {
            var routineSyncJobId = RoutineSyncJobId();
            
            using (var transaction = _stateRepository.BeginTransaction())
            {
                var state = _stateRepository.RetrieveSystemState();

                RecurringJob.AddOrUpdate<BackgroundJobRunner>(  
                    routineSyncJobId,
                    x => x.RunRealTimeSynchronization(_tenantContext.InstanceId),
                    "*/1 * * * *",
                    TimeZoneInfo.Utc);

                state.RealTimeHangFireJobId = routineSyncJobId;;
                _connectionRepository.Entities.SaveChanges();

                RecurringJob.Trigger(routineSyncJobId);
                transaction.Commit();
            }
        }

        public bool IsRealTimeSyncRunning()
        {
            var state = _stateRepository.RetrieveSystemState();
            return !state.RealTimeHangFireJobId.IsNullOrEmpty();
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
    }
}

