using System;
using System.Collections.Generic;
using Monster.Middle.Persist.Multitenant;
using Monster.Middle.Processes.Sync;
using Monster.Middle.Processes.Sync.Inventory.Model;
using Monster.Middle.Security;
using Push.Foundation.Utilities.Logging;


namespace Monster.Middle.Hangfire
{
    public class ExclusiveJobRunner
    {
        private readonly SyncDirector _director;
        private readonly ConnectionContext _connectionContext;
        private readonly StateRepository _stateRepository;
        private readonly IPushLogger _logger;
        
        public ExclusiveJobRunner(
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

        // Named locks
        //
        static readonly NamedLock RealTimeSyncLock = new NamedLock("RealTimeSync");
        static readonly NamedLock InventoryPullLock = new NamedLock("InventoryPull");
        static readonly NamedLock ImportIntoAcumaticaLock = new NamedLock("ImportIntoAcumatica");
        

        // Long-running Jobs with Named Lock protection
        //
        public void PullInventory(Guid instanceId)
        {
            RunOneTaskPerInstance(
                instanceId, _director.PullInventory, InventoryPullLock, BackgroundJobType.PullInventory);
        }
        
        public void ImportIntoAcumatica(
                    Guid instanceId, AcumaticaInventoryImportContext context)
        {
            RunOneTaskPerInstance(
                instanceId, 
                () => _director.ImportIntoAcumatica(context), 
                ImportIntoAcumaticaLock,
                BackgroundJobType.ImportIntoAcumatica);
        }
        
        public void RealTimeSync(Guid instanceId)
        {
            RunOneTaskPerInstance(
                instanceId, () => _director.RealTimeSynchronization(), RealTimeSyncLock);
        }




        private void RunOneTaskPerInstance(
                Guid instanceId, Action action, NamedLock methodLock, int? jobType = null)
        {
            try
            {
                if (!methodLock.Acquire(instanceId.ToString()))
                {
                    var msg = $"Failed to acquire {methodLock.MethodName} lock for {instanceId}";
                    _logger.Info(msg);
                    return;
                }

                _connectionContext.Initialize(instanceId);

                action();
                

                // Cleans up the Background Job record
                if (jobType.HasValue)
                {
                    _stateRepository.RemoveBackgroundJobs(jobType.Value);
                }

                // *** Important - do not refactor this to use finally, else it will 
                // ... break concurrency locking
                //
                methodLock.Free(instanceId.ToString());
            }
            catch (Exception ex)
            {
                // Cleans up the Background Job record
                if (jobType.HasValue)
                {
                    _stateRepository.RemoveBackgroundJobs(jobType.Value);
                }

                methodLock.Free(instanceId.ToString());
                _logger.Error(ex);
                throw;
            }
        }

    }
}

