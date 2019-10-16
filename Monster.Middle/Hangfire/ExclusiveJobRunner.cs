using System;
using Monster.Middle.Persist.Instance;
using Monster.Middle.Processes.Misc;
using Monster.Middle.Processes.Sync.Managers;
using Monster.Middle.Processes.Sync.Model.Inventory;
using Monster.Middle.Processes.Sync.Services;
using Push.Foundation.Utilities.Logging;


namespace Monster.Middle.Hangfire
{
    public class ExclusiveJobRunner
    {
        private readonly ProcessDirector _processDirector;
        private readonly InstanceContext _instanceContext;
        private readonly ExclusiveJobMonitoringService _jobMonitoringService;
        private readonly ExecutionLogService _executionLogService;
        private readonly IPushLogger _logger;

        // Allows only one Background Job to be executed at a time per instance
        //
        static readonly NamedLock InstanceLock = new NamedLock("InstanceLock");

        public ExclusiveJobRunner(
                ProcessDirector processDirector, 
                InstanceContext instanceContext,
                ExclusiveJobMonitoringService jobMonitoringService, 
                IPushLogger logger, 
                ExecutionLogService executionLogService)
        {
            _processDirector = processDirector;
            _instanceContext = instanceContext;
            _jobMonitoringService = jobMonitoringService;
            _logger = logger;
            _executionLogService = executionLogService;
        }


        public void RunConnectToAcumatica(Guid instanceId)
        {
            ExecuteOnePerInstance(
                instanceId, _processDirector.ConnectToAcumatica, ExclusiveJobType.ConnectToAcumatica);
        }

        public void RunPullAcumaticaRefData(Guid instanceId)
        {
            ExecuteOnePerInstance(
                instanceId, _processDirector.RefreshAcumaticaRefData, ExclusiveJobType.PullAcumaticaRefData);
        }

        public void RunSyncWarehouseAndLocation(Guid instanceId)
        {
            ExecuteOnePerInstance(
                instanceId, _processDirector.SyncWarehouseAndLocation, ExclusiveJobType.SyncWarehouseAndLocation);
        }

        public void RunDiagnostics(Guid instanceId)
        {
            ExecuteOnePerInstance(
                instanceId, _processDirector.RunDiagnostics, ExclusiveJobType.Diagnostics);
        }
        
        public void PullInventory(Guid instanceId)
        {
            ExecuteOnePerInstance(instanceId, _processDirector.RefreshInventory, ExclusiveJobType.PullInventory);
        }
        
        public void ImportIntoAcumatica(Guid instanceId, AcumaticaInventoryImportContext context)
        {
            ExecuteOnePerInstance(
                instanceId, 
                () => _processDirector.ImportInventoryToAcumatica(context), 
                ExclusiveJobType.ImportIntoAcumatica);
        }
        
        public void EndToEndSync(Guid instanceId)
        {
            ExecuteOnePerInstance(
                instanceId, () => _processDirector.EndToEndSync(), ExclusiveJobType.EndToEndSync);
        }
        


        private void ExecuteOnePerInstance(Guid instanceId, Action action, int jobType)
        {
            try
            {
                _instanceContext.Initialize(instanceId);

                if (!InstanceLock.Acquire(instanceId.ToString()))
                {
                    var msg = $"Failed to acquire lock '{InstanceLock.MethodName}' for {instanceId}";
                    _executionLogService.InsertExecutionLog(msg, LogLevel.Debug);
                    return;
                }

                // Execute the requested task
                //
                action();
                
                // If this is One-Time Job, this will remove the Monitor now that the Job is completed
                //
                _jobMonitoringService.RemoveOneTimeJobMonitor(jobType);

                // *** IMPORTANT - do not refactor this to use-finally, else it will 
                // ... break concurrency locking
                //
                InstanceLock.Free(instanceId.ToString());
            }
            catch (Exception ex)
            {
                InstanceLock.Free(instanceId.ToString());

                // If this is One-Time Job, this will remove the Monitor now that the Job has failed
                //
                _jobMonitoringService.RemoveOneTimeJobMonitor(jobType);

                _logger.Error(ex);
                throw;
            }
        }

    }
}

