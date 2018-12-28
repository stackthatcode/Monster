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
        private readonly TenantContext _tenantContext;
        private readonly StateRepository _stateRepository;
        private readonly IPushLogger _logger;
        
        

        public BackgroundJobRunner(
                SyncDirector director, 
                TenantContext tenantContext, 
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
        public void RunConnectToAcumatica(Guid tenantId)
        {
            FireAndForgetJob(tenantId,
                BackgroundJobType.ConnectToAcumatica,
                _director.ConnectToAcumatica);
        }

        public void RunPullAcumaticaSettings(Guid tenantId)
        {
            FireAndForgetJob(tenantId,
                BackgroundJobType.PullAcumaticaReferenceData,
                _director.PullAcumaticaReferenceData);
        }

        public void RunSyncWarehouseAndLocation(Guid tenantId)
        {
            FireAndForgetJob(tenantId,
                BackgroundJobType.SyncWarehouseAndLocation, 
                _director.SyncWarehouseAndLocation);
        }

        public void RunLoadInventoryIntoAcumatica(Guid tenantId)
        {
            FireAndForgetJob(tenantId,
                BackgroundJobType.PushInventoryToAcumatica,
                _director.LoadInventoryIntoAcumatica);
        }

        public void RunLoadInventoryIntoShopify(Guid tenantId)
        {
            FireAndForgetJob(tenantId,
                BackgroundJobType.PushInventoryToShopify,
                _director.LoadInventoryIntoShopify);
        }
        
        // FaF Background Jobs do their own error handling as that is 
        // ... reflected in System State
        private void FireAndForgetJob(
                Guid tenantId, int queueJobTypeId, Action task)
        {
            _tenantContext.Initialize(tenantId);
            task();
            _stateRepository.RemoveBackgroundJobs(queueJobTypeId);
        }



        // Routine Sync
        //
        static readonly NamedLock 
                RoutineSyncLock = new NamedLock("RoutineSynchHarness");
        
        public void RunRoutineSync(Guid tenantId)
        {
            try
            {
                if (!RoutineSyncLock.Acquire(tenantId.ToString()))
                {
                    _logger.Info($"Failed to acquired RoutineSynchHarness for {tenantId}");
                    return;
                }

                _tenantContext.Initialize(tenantId);
                _director.RoutineSync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                throw;
            }
            finally
            {
                RoutineSyncLock.Free(tenantId.ToString());
            }
        }

    }
}
