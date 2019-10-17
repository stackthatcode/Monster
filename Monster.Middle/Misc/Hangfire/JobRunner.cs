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
            ExecuteJob(instanceId, _processDirector.RefreshAcumaticaRefData, jobMonitorId);
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
                Guid instanceId, AcumaticaInventoryImportContext context, long jobMonitorId)
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
                    var msg = $"Failed to acquire lock '{InstanceLock.MethodName}' for {instanceId}";
                    _executionLogService.InsertExecutionLog(msg, LogLevel.Debug);
                    return;
                }

                if (_jobMonitoringService.IsCorrupted(jobMonitorId))
                {
                    var msg = $"Job Monitor {jobMonitorId} is missing or corrupted";
                    _executionLogService.InsertExecutionLog(msg);
                    _jobMonitoringService.CleanupPostExecution(jobMonitorId);
                    return;
                }

                if (_jobMonitoringService.IsMissingOrReceivedKillSignal(jobMonitorId))
                {
                    var msg = $"Job Monitor {jobMonitorId} has received signal";
                    _executionLogService.InsertExecutionLog(msg);
                    _jobMonitoringService.CleanupPostExecution(jobMonitorId);
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

