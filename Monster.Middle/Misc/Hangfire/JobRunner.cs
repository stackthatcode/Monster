using System;
using Monster.Middle.Misc.Logging;
using Monster.Middle.Persist.Instance;
using Monster.Middle.Processes.Sync.Managers;
using Monster.Middle.Processes.Sync.Model.Inventory;
using Push.Foundation.Utilities.Logging;

namespace Monster.Middle.Misc.Hangfire
{
    public class JobRunner
    {
        private readonly ProcessDirector _processDirector;
        private readonly InstanceContext _instanceContext;
        private readonly JobMonitoringService _jobMonitoringService;
        private readonly ExecutionLogService _executionLogService;
        private readonly IPushLogger _logger;

        public JobRunner(
                ProcessDirector processDirector, 
                InstanceContext instanceContext,
                JobMonitoringService jobMonitoringService, 
                ExecutionLogService executionLogService,
                IPushLogger logger)
        {
            _processDirector = processDirector;
            _instanceContext = instanceContext;
            _jobMonitoringService = jobMonitoringService;
            _executionLogService = executionLogService;
            _logger = logger;
        }


        public void ConnectToAcumatica(Guid instanceId, long jobMonitorId)
        {
            ExecuteJob(instanceId, _processDirector.ConnectToAcumatica, jobMonitorId);
        }

        public void RefreshAcumaticaRefData(Guid instanceId, long jobMonitorId)
        {
            ExecuteJob(instanceId, _processDirector.RefreshReferenceData, jobMonitorId);
        }

        public void SyncWarehouseAndLocation(Guid instanceId, long jobMonitorId)
        {
            ExecuteJob(instanceId, _processDirector.SyncWarehouseAndLocation, jobMonitorId);
        }

        public void Diagnostics(Guid instanceId, long jobMonitorId)
        {
            ExecuteJob(instanceId, _processDirector.RunDiagnostics, jobMonitorId);
        }
        
        public void RefreshInventory(Guid instanceId, long jobMonitorId)
        {
            ExecuteJob(instanceId, _processDirector.RefreshInventory, jobMonitorId);
        }
        
        public void ImportIntoAcumatica(
                Guid instanceId, AcumaticaStockItemImportContext context, long jobMonitorId)
        {
            ExecuteJob(
                instanceId, () => _processDirector.ImportInventoryToAcumatica(context), jobMonitorId);
        }
        
        public void EndToEndSync(Guid instanceId, long jobMonitorId)
        {
            ExecuteJob(instanceId, () => _processDirector.EndToEndSync(), jobMonitorId);
        }



        // Allows only one Exclusive Job at a time to be executed per instance
        //
        static readonly NamedLock InstanceLock = new NamedLock("ExecuteJob");

        private void ExecuteJob(Guid instanceId, Action action, long jobMonitorId)
        {
            try
            {
                _instanceContext.Initialize(instanceId);

                if (!InstanceLock.Acquire(instanceId.ToString()))
                {
                    var msg = $"Failed to acquire lock '{InstanceLock.MethodName}'";
                    _executionLogService.Log(msg, LogLevel.Debug);
                    return;
                }

                if (_jobMonitoringService.IsCorrupted(jobMonitorId))
                {
                    var msg = $"Job Monitor {jobMonitorId} is missing or corrupted";
                    _executionLogService.Log(msg);
                    _jobMonitoringService.CleanupPostExecution(jobMonitorId);
                    InstanceLock.Free(instanceId.ToString());
                    return;
                }

                if (_jobMonitoringService.IsMissingOrReceivedKillSignal(jobMonitorId))
                {
                    var msg = $"Job Monitor {jobMonitorId} has received kill signal";
                    _executionLogService.Log(msg);
                    _jobMonitoringService.CleanupPostExecution(jobMonitorId);
                    InstanceLock.Free(instanceId.ToString());
                    return;
                }

                // Phew - we made it! Execute the requested task
                //
                action();

                _jobMonitoringService.CleanupPostExecution(jobMonitorId);

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
                _jobMonitoringService.CleanupPostExecution(jobMonitorId);

                _logger.Error(ex);
                throw;
            }
        }
    }
}

