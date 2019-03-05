using System;
using Monster.Middle.Persist.Master;
using Monster.Middle.Processes.Sync.Managers;
using Monster.Middle.Processes.Sync.Model.Inventory;
using Monster.Middle.Processes.Sync.Services;
using Push.Foundation.Utilities.Logging;


namespace Monster.Middle.Hangfire
{
    public class JobRunner
    {
        private readonly SyncDirector _director;
        private readonly ConnectionContext _connectionContext;
        private readonly SystemStateRepository _stateRepository;
        private readonly IPushLogger _logger;
        
        public JobRunner(
                SyncDirector director, 
                ConnectionContext connectionContext, 
                SystemStateRepository stateRepository,
                IPushLogger logger)
        {
            _director = director;
            _connectionContext = connectionContext;
            _stateRepository = stateRepository;
            _logger = logger;
        }

        // Named locks
        //
        static readonly NamedLock ConnectToAcumaticaLock = new NamedLock("ConnectToAcumatica");
        static readonly NamedLock PullAcumaticaRefData = new NamedLock("PullAcumaticaRefData");
        static readonly NamedLock SyncWarehouseAndLocation = new NamedLock("SyncWarehouseAndLocation");
        static readonly NamedLock Diagnostics = new NamedLock("Diagnostics");         
        static readonly NamedLock InventoryPullLock = new NamedLock("InventoryPull");
        static readonly NamedLock ImportIntoAcumaticaLock = new NamedLock("ImportIntoAcumatica");
        static readonly NamedLock RealTimeSyncLock = new NamedLock("RealTimeSync");

        // Generic Background Job Lock
        //
        static readonly NamedLock ExecutionLock = new NamedLock("ExecutionLock");



        // Fire-and-Forget Jobs
        //
        public void RunConnectToAcumatica(Guid instanceId)
        {
            ExecuteOnePerInstance(
                instanceId, 
                _director.ConnectToAcumatica, 
                ExecutionLock, 
                BackgroundJobType.ConnectToAcumatica);
        }

        public void RunPullAcumaticaRefData(Guid instanceId)
        {
            ExecuteOnePerInstance(
                instanceId, 
                _director.PullAcumaticaRefData, 
                ExecutionLock, 
                BackgroundJobType.PullAcumaticaRefData);
        }

        public void RunSyncWarehouseAndLocation(Guid instanceId)
        {
            ExecuteOnePerInstance(
                instanceId, 
                _director.SyncWarehouseAndLocation, 
                ExecutionLock, 
                BackgroundJobType.SyncWarehouseAndLocation);
        }

        public void RunDiagnostics(Guid instanceId)
        {
            ExecuteOnePerInstance(
                instanceId,
                _director.RunDiagnostics, 
                ExecutionLock, 
                BackgroundJobType.Diagnostics);
        }
        

        // Long-running Jobs with Named Lock protection
        //
        public void PullInventory(Guid instanceId)
        {
            ExecuteOnePerInstance(
                instanceId, 
                _director.PullInventory, 
                ExecutionLock, 
                BackgroundJobType.PullInventory);
        }
        
        public void ImportIntoAcumatica(Guid instanceId, AcumaticaInventoryImportContext context)
        {
            ExecuteOnePerInstance(
                instanceId, 
                () => _director.ImportIntoAcumatica(context),
                ExecutionLock,
                BackgroundJobType.ImportIntoAcumatica);
        }
        
        public void RealTimeSync(Guid instanceId)
        {
            ExecuteOnePerInstance(instanceId, () => _director.FullSync(), ExecutionLock);
        }




        private void ExecuteOnePerInstance(
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

