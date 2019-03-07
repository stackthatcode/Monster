using System;
using Hangfire;
using Monster.Middle.Persist.Master;
using Monster.Middle.Persist.Tenant;
using Monster.Middle.Processes.Sync.Persist;
using Monster.Middle.Processes.Sync.Services;
using Push.Foundation.Utilities.Helpers;
using Push.Foundation.Utilities.Logging;


namespace Monster.Middle.Hangfire
{
    public class RecurringJobService
    {
        private readonly ConnectionContext _tenantContext;
        private readonly ConnectionRepository _connectionRepository;
        private readonly SystemStateRepository _stateRepository;
        private readonly ExecutionLogService _executionLogService;


        public RecurringJobService(
            IPushLogger logger,
            ConnectionContext tenantContext,
            ConnectionRepository connectionRepository,
            SystemStateRepository stateRepository, 
            ExecutionLogService executionLogService)
        {
            _tenantContext = tenantContext;
            _connectionRepository = connectionRepository;
            _stateRepository = stateRepository;
            _executionLogService = executionLogService;
        }

        public void StartRoutineSync()
        {
            var routineSyncJobId = RoutineSyncJobId();
            using (var transaction = _stateRepository.BeginTransaction())
            {
                var state = _stateRepository.RetrieveSystemState();

                RecurringJob.AddOrUpdate<JobRunner>(
                    routineSyncJobId,
                    x => x.RealTimeSync(_tenantContext.InstanceId),
                    "*/1 * * * *",
                    TimeZoneInfo.Utc);

                state.RealTimeHangFireJobId = routineSyncJobId;
                
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

                _executionLogService.InsertExecutionLog("Real-Time Sync - Initiating Pause");
                transaction.Commit();
            }
        }

        private string RoutineSyncJobId()
        {
            return "RoutineSync:" + _tenantContext.InstanceId;
        }
    }
}

