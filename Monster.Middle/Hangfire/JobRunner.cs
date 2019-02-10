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
            FireAndForgetJob(instanceId,
                JobType.ConnectToAcumatica,
                _director.ConnectToAcumatica);
        }

        public void RunPullAcumaticaRefData(Guid instanceId)
        {
            FireAndForgetJob(instanceId,
                JobType.PullAcumaticaRefData,
                _director.PullAcumaticaReferenceData);
        }

        public void RunSyncWarehouseAndLocation(Guid instanceId)
        {
            FireAndForgetJob(instanceId,
                JobType.SyncWarehouseAndLocation, 
                _director.SyncWarehouseAndLocation);
        }

        public void RunDiagnostics(Guid instanceId)
        {
            FireAndForgetJob(instanceId,
                JobType.Diagnostics,
                _director.RunDiagnostics);
        }
        
        public void PushInventoryToAcumatica(Guid instanceId)
        {
            // Longer running processes - we'll use NamedLocks
            //
            FireAndForgetJob(
                instanceId,
                JobType.PushInventoryToAcumatica,
                _director.LoadInventoryIntoAcumatica,
                InventorySyncLock);
        }

        public void PushInventoryToShopify(Guid instanceId)
        {
            // Longer running processes - we'll use NamedLocks
            //
            FireAndForgetJob(
                instanceId,
                JobType.PushInventoryToShopify,
                _director.LoadInventoryIntoShopify,
                InventorySyncLock);
        }

        public void RealTimeSynchronization(Guid instanceId)
        {
            _connectionContext.Initialize(instanceId);

            RunOneTaskPerInstance(
                instanceId, RealTimeSyncLock, () => _director.RealTimeSynchronization());
        }


        
        // FaF Background Jobs do their own error handling, as reflected in System State
        //
        private void FireAndForgetJob(Guid instanceId, int queueJobTypeId, Action task)
        {
            _connectionContext.Initialize(instanceId);
            task();
            _stateRepository.RemoveBackgroundJobs(queueJobTypeId);
        }

        private void FireAndForgetJob(
                Guid instanceId, int queueJobTypeId, Action task, NamedLock namedLock)
        {
            _connectionContext.Initialize(instanceId);
            RunOneTaskPerInstance(instanceId, namedLock, () => task());
            _stateRepository.RemoveBackgroundJobs(queueJobTypeId);
        }


        // Concurrent task locking for longer running jobs
        //
        static readonly NamedLock RealTimeSyncLock = new NamedLock("RealTimeSync");
        static readonly NamedLock InventorySyncLock = new NamedLock("InventorySync");

        private void RunOneTaskPerInstance(
                Guid instanceId, NamedLock methodLock, Action action)
        {
            try
            {
                if (!methodLock.Acquire(instanceId.ToString()))
                {
                    var msg = $"Failed to acquire {methodLock.MethodName} lock for {instanceId}";
                    _logger.Info(msg);
                    return;
                }

                action();

                // *** Important - do not refactor this to use finally, else it will 
                // ... break concurrency locking
                //
                methodLock.Free(instanceId.ToString());
            }
            catch (Exception ex)
            {
                methodLock.Free(instanceId.ToString());
                _logger.Error(ex);
                throw;
            }
        }
    }
}

