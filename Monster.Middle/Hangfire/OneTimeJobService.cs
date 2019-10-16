using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Monster.Middle.Persist.Instance;
using Monster.Middle.Processes.Sync.Model.Inventory;
using BackgroundJob = Hangfire.BackgroundJob;


namespace Monster.Middle.Hangfire
{
    public class OneTimeJobService
    {
        private readonly InstanceContext _tenantContext;
        private readonly ExclusiveJobMonitoringService _jobMonitoringService;


        public OneTimeJobService(
                InstanceContext tenantContext, ExclusiveJobMonitoringService jobMonitoringService)
        {
            _tenantContext = tenantContext;
            _jobMonitoringService = jobMonitoringService;
        }


        public void ConnectToAcumatica()
        {
            QueueJob(ExclusiveJobType.ConnectToAcumatica,
                x => x.RunConnectToAcumatica(_tenantContext.InstanceId));
        }

        public void PullAcumaticaRefData()
        {
            QueueJob(ExclusiveJobType.PullAcumaticaRefData, 
                x => x.RunPullAcumaticaRefData(_tenantContext.InstanceId));
        }

        public void SyncWarehouseAndLocation()
        {
            QueueJob(ExclusiveJobType.SyncWarehouseAndLocation,
                x => x.RunSyncWarehouseAndLocation(_tenantContext.InstanceId));
        }
    
        public void RunDiagnostics()
        {
            QueueJob(ExclusiveJobType.Diagnostics, x => x.RunDiagnostics(_tenantContext.InstanceId));
        }

        public void PullInventory()
        {
            QueueJob(ExclusiveJobType.PullInventory, x => x.PullInventory(_tenantContext.InstanceId));
        }
        
        public void ImportIntoAcumatica(
                List<long> spids, bool createInventoryReceipts, bool automaticEnable)
        {
            var context = new AcumaticaInventoryImportContext
            {
                ShopifyProductIds = spids,
                CreateInventoryReceipts = createInventoryReceipts,
                IsSyncEnabled = automaticEnable,
            };

            QueueJob(ExclusiveJobType.ImportIntoAcumatica,
                    x => x.ImportIntoAcumatica(_tenantContext.InstanceId, context));
        }
        


        // Worker methods
        //
        private void QueueJob(int jobType, Expression<Action<ExclusiveJobRunner>> action)
        {
            var monitoringDigest = _jobMonitoringService.GetMonitoringDigest();

            if (monitoringDigest.AreAnyJobsActive)
            {
                return;
            }

            using (var transaction = _jobMonitoringService.BeginTransaction())
            {
                var jobId = BackgroundJob.Enqueue<ExclusiveJobRunner>(action);
                var monitor = _jobMonitoringService.AddJobMonitor(jobType, jobId);
                monitor.HangFireJobId = jobId;

                transaction.Commit();
            }
        }
    }
}

