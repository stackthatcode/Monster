using System;
using Monster.Middle.Persist.Multitenant;
using Monster.Middle.Processes.Sync;
using Monster.Middle.Security;
using Push.Foundation.Utilities.Logging;


namespace Monster.Middle.Hangfire
{
    public class JobRunner
    {
        private readonly SyncDirector _director;
        private readonly ConnectionContext _connectionContext;
        private readonly StateRepository _stateRepository;
        private readonly IPushLogger _logger;
        
        public JobRunner(
                SyncDirector director, 
                ConnectionContext connectionContext, 
                StateRepository stateRepository,
                IPushLogger logger)
        {
            _director = director;
            _connectionContext = connectionContext;
            _stateRepository = stateRepository;
            _logger = logger;
        }


        // Fire-and-Forget Jobs
        //
        public void RunConnectToAcumatica(Guid instanceId)
        {
            FireAndForgetJob(instanceId, BackgroundJobType.ConnectToAcumatica, _director.ConnectToAcumatica);
        }

        public void RunPullAcumaticaRefData(Guid instanceId)
        {
            FireAndForgetJob(instanceId, BackgroundJobType.PullAcumaticaRefData, _director.PullAcumaticaReferenceData);
        }

        public void RunSyncWarehouseAndLocation(Guid instanceId)
        {
            FireAndForgetJob(
                instanceId, BackgroundJobType.SyncWarehouseAndLocation, _director.SyncWarehouseAndLocation);
        }

        public void RunDiagnostics(Guid instanceId)
        {
            FireAndForgetJob(instanceId, BackgroundJobType.Diagnostics, _director.RunDiagnostics);
        }
        
        
        
        // FaF Background Jobs do their own error handling, as reflected in System State
        //
        private void FireAndForgetJob(Guid instanceId, int jobType, Action task)
        {
            _connectionContext.Initialize(instanceId);
            task();
            _stateRepository.RemoveBackgroundJobs(jobType);
        }
    }
}

