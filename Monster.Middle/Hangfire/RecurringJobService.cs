using System;
using Hangfire;
using Monster.Middle.Persist.Multitenant;
using Monster.Middle.Security;
using Push.Foundation.Utilities.Helpers;
using Push.Foundation.Utilities.Logging;


namespace Monster.Middle.Hangfire
{
    public class RecurringJobService
    {
        private readonly ConnectionContext _tenantContext;
        private readonly ConnectionRepository _connectionRepository;
        private readonly StateRepository _stateRepository;
        private readonly IPushLogger _logger;


        public RecurringJobService(
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

        
        public void StartRoutineSync()
        {
            var routineSyncJobId = RealTimeSyncJobIdGenerator(); 
            
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

        private string RealTimeSyncJobIdGenerator()
        {
            return "RealTimeSync:" + _tenantContext.InstanceId;
        }
        
        public bool IsRealTimeSyncRunning()
        {
            var state = _stateRepository.RetrieveSystemState();
            return !state.RealTimeHangFireJobId.IsNullOrEmpty();
        }
    }
}

