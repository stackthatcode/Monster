using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Monster.Middle.Directors;
using Monster.Middle.Persist.Multitenant;
using Monster.Middle.Persist.Multitenant.Model;
using Monster.Middle.Processes.Sync;
using Monster.Middle.Processes.Sync.Directors;
using Monster.Middle.Processes.Sync.Inventory.Services;
using Monster.Middle.Services;
using Push.Foundation.Utilities.Logging;

namespace Monster.Middle.Jobs
{
    public class JobRunner
    {
        private readonly SyncDirector _director;
        private readonly TenantContext _tenantContext;
        private readonly TenantRepository _tenantRepository;
        private readonly JobRepository _jobRepository;
        private readonly IPushLogger _logger;
        
        

        public JobRunner(
                SyncDirector director, 
                TenantContext tenantContext, 
                TenantRepository tenantRepository, 
                InventoryStatusService inventoryStatusService, 
                JobRepository jobRepository, 
                IPushLogger logger)
        {
            _director = director;
            _tenantContext = tenantContext;
            _tenantRepository = tenantRepository;
            _jobRepository = jobRepository;
            _logger = logger;
        }

        
        public void ClearFireAndForgetJob(Guid tenantId)
        {
            _tenantContext.Initialize(tenantId);
            _jobRepository.Clear();
        }

        public void RunSyncWarehouseAndLocation(Guid tenantId)
        {
            RunFireAndForgetJob(
                tenantId,
                QueuedJobType.SyncWarehouseAndLocation, 
                _director.SyncWarehouseAndLocation);
        }

        public void RunLoadInventoryIntoAcumatica(Guid tenantId)
        {
            RunFireAndForgetJob(
                tenantId,
                QueuedJobType.LoadInventoryIntoAcumatica,
                _director.LoadInventoryIntoAcumatica);
        }

        public void RunLoadInventoryIntoShopify(Guid tenantId)
        {
            RunFireAndForgetJob(
                tenantId,
                QueuedJobType.LoadInventoryIntoShopify,
                _director.LoadInventoryIntoShopify);
        }

        private void RunFireAndForgetJob(
                Guid tenantId, int queueJobTypeId, Action task)
        {
            try
            {
                _tenantContext.Initialize(tenantId);
                task();
                _jobRepository.UpdateStatus(queueJobTypeId, JobStatus.Complete);
            }
            catch (Exception ex)
            {
                _jobRepository.UpdateStatus(queueJobTypeId, JobStatus.Failed);
                _logger.Error(ex);
                throw;
            }
        }


        static readonly NamedLock _routineSyncLock = new NamedLock("RoutineSynchHarness");


        // TODO - add NameLocking based on Tenant Id
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
