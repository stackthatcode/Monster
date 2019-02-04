using System;
using Monster.Middle.Persist.Multitenant;
using Monster.Middle.Processes.Sync;
using Monster.Middle.Security;
using Push.Foundation.Utilities.Logging;


namespace Monster.Middle.Hangfire
{
    public class BackgroundJobRunner
    {
        private readonly SyncDirector _director;
        private readonly ConnectionContext _tenantContext;
        private readonly StateRepository _stateRepository;
        private readonly IPushLogger _logger;
        

        public BackgroundJobRunner(
                SyncDirector director, 
                ConnectionContext tenantContext, 
                StateRepository stateRepository,
                IPushLogger logger)
        {
            _director = director;
            _tenantContext = tenantContext;
            _stateRepository = stateRepository;
            _logger = logger;
        }


        // Fire-and-Forget Jobs
        //
        public void RunConnectToAcumatica(Guid instanceId)
        {
            FireAndForgetJob(instanceId,
                BackgroundJobType.ConnectToAcumatica,
                _director.ConnectToAcumatica);
        }

        public void RunPullAcumaticaRefData(Guid instanceId)
        {
            FireAndForgetJob(instanceId,
                BackgroundJobType.PullAcumaticaRefData,
                _director.PullAcumaticaReferenceData);
        }

        public void RunSyncWarehouseAndLocation(Guid instanceId)
        {
            FireAndForgetJob(instanceId,
                BackgroundJobType.SyncWarehouseAndLocation, 
                _director.SyncWarehouseAndLocation);
        }

        public void RunDiagnostics(Guid instanceId)
        {
            FireAndForgetJob(instanceId,
                BackgroundJobType.Diagnostics,
                _director.RunDiagnostics);
        }


        // Longer running processes - we'll use NamedLocks
        //
        public void PushInventoryToAcumatica(Guid instanceId)
        {
            FireAndForgetJob(
                instanceId,
                BackgroundJobType.PushInventoryToAcumatica,
                _director.LoadInventoryIntoAcumatica,
                InventorySyncLock);
        }

        public void PushInventoryToShopify(Guid instanceId)
        {
            FireAndForgetJob(
                instanceId,
                BackgroundJobType.PushInventoryToShopify,
                _director.LoadInventoryIntoShopify,
                InventorySyncLock);
        }

        public void RealTimeSynchronization(Guid instanceId)
        {
            _tenantContext.Initialize(instanceId);

            RunOneTaskPerInstance(
                instanceId, RealTimeSyncLock, () => _director.RealTimeSynchronization());
        }



        // Execution plumbing
        //

        // FaF Background Jobs do their own error handling, as reflected in System State
        //
        private void FireAndForgetJob(Guid instanceId, int queueJobTypeId, Action task)
        {
            _tenantContext.Initialize(instanceId);
            task();
            _stateRepository.RemoveBackgroundJobs(queueJobTypeId);
        }

        private void FireAndForgetJob(
                Guid instanceId, int queueJobTypeId, Action task, NamedLock namedLock)
        {
            _tenantContext.Initialize(instanceId);
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
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                throw;
            }
            finally
            {
                methodLock.Free(instanceId.ToString());
            }
        }
    }
}

