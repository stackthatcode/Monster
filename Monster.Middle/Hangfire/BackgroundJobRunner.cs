using System;
using Monster.Middle.Persist.Multitenant;
using Monster.Middle.Persist.Multitenant.Model;
using Monster.Middle.Processes.Sync.Directors;
using Monster.Middle.Processes.Sync.Inventory.Services;
using Monster.Middle.Services;
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
        

        public void RunSyncWarehouseAndLocation(Guid tenantId)
        {
            FireAndForgetJob(
                tenantId,
                BackgroundJobType.SyncWarehouseAndLocation, 
                _director.SyncWarehouseAndLocation);
        }

        public void RunLoadInventoryIntoAcumatica(Guid tenantId)
        {
            FireAndForgetJob(
                tenantId,
                BackgroundJobType.PushInventoryToAcumatica,
                _director.LoadInventoryIntoAcumatica);
        }

        public void RunLoadInventoryIntoShopify(Guid tenantId)
        {
            FireAndForgetJob(
                tenantId,
                BackgroundJobType.PushInventoryToShopify,
                _director.LoadInventoryIntoShopify);
        }


        private void FireAndForgetJob(
                Guid tenantId, 
                int queueJobTypeId, 
                Action task)
        {
            try
            {
                _tenantContext.Initialize(tenantId);
                task();
                _stateRepository.RemoveBackgroundJob(queueJobTypeId);
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                throw;
            }
        }


        static readonly NamedLock _routineSyncLock = new NamedLock("RoutineSynchHarness");
        
        public void RunRoutineSync(Guid tenantId)
        {
            try
            {
                if (!_routineSyncLock.Acquire(tenantId.ToString()))
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
                _routineSyncLock.Free(tenantId.ToString());
            }
        }

    }
}
