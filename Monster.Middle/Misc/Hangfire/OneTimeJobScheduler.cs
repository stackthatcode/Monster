using System.Collections.Generic;
using Monster.Middle.Misc.Logging;
using Monster.Middle.Persist.Instance;
using Monster.Middle.Processes.Sync.Model.Inventory;
using BackgroundJob = Hangfire.BackgroundJob;


namespace Monster.Middle.Misc.Hangfire
{
    public class OneTimeJobScheduler
    {
        private readonly InstanceContext _tenantContext;
        private readonly JobMonitoringService _jobMonitoringService;
        private readonly ExecutionLogService _executionLogService;

        public OneTimeJobScheduler(
                InstanceContext tenantContext, 
                JobMonitoringService jobMonitoringService, 
                ExecutionLogService executionLogService)
        {
            _tenantContext = tenantContext;
            _jobMonitoringService = jobMonitoringService;
            _executionLogService = executionLogService;
        }

        public void ConnectToAcumatica()
        {
            var monitor = _jobMonitoringService
                .ProvisionJobMonitor(BackgroundJobType.ConnectToAcumatica, false);
            
            var hangfireJobId = BackgroundJob.Enqueue<JobRunner>(
                    x => x.ConnectToAcumatica(_tenantContext.InstanceId, monitor.Id));

            _jobMonitoringService.AssignHangfireJob(monitor.Id, hangfireJobId);
        }

        public void RefreshAcumaticaReferenceData()
        {
            var monitor = _jobMonitoringService
                .ProvisionJobMonitor(BackgroundJobType.RefreshAcumaticaRefData, false);

            var hangfireJobId = BackgroundJob.Enqueue<JobRunner>(
                    x => x.RefreshAcumaticaRefData(_tenantContext.InstanceId, monitor.Id));

            _jobMonitoringService.AssignHangfireJob(monitor.Id, hangfireJobId);
        }

        public void SyncWarehouseAndLocation()
        {
            var monitor = _jobMonitoringService
                .ProvisionJobMonitor(BackgroundJobType.SyncWarehouseAndLocation, false);

            var hangfireJobId = BackgroundJob.Enqueue<JobRunner>(
                    x => x.SyncWarehouseAndLocation(_tenantContext.InstanceId, monitor.Id));

            _jobMonitoringService.AssignHangfireJob(monitor.Id, hangfireJobId);
        }
        
        public void RunDiagnostics()
        {
            var monitor = _jobMonitoringService
                .ProvisionJobMonitor(BackgroundJobType.Diagnostics, false);

            var hangfireJobId = BackgroundJob.Enqueue<JobRunner>(
                    x => x.Diagnostics(_tenantContext.InstanceId, monitor.Id));

            _jobMonitoringService.AssignHangfireJob(monitor.Id, hangfireJobId);
        }

        public void RefreshInventory()
        {
            var monitor = _jobMonitoringService
                .ProvisionJobMonitor(BackgroundJobType.RefreshInventory, false);

            var hangfireJobId = BackgroundJob.Enqueue<JobRunner>(
                    x => x.RefreshInventory(_tenantContext.InstanceId, monitor.Id));

            _jobMonitoringService.AssignHangfireJob(monitor.Id, hangfireJobId);
        }
        
        public void ImportAcumaticaStockItems(
                List<long> spids, bool createReceipts, string warehouseId, bool automaticEnable)
        {
            var context = new AcumaticaStockItemImportContext
            {
                ShopifyProductIds = spids,
                CreateInventoryReceipts = createReceipts,
                IsSyncEnabled = automaticEnable,
                WarehouseId = warehouseId,
                SynchronizeOnly = false,
            };

            var monitor = _jobMonitoringService
                .ProvisionJobMonitor(BackgroundJobType.ImportAcumaticaStockItems, false);

            var hangfireJobId = BackgroundJob.Enqueue<JobRunner>(
                    x => x.ImportAcumaticaStockItems(_tenantContext.InstanceId, context, monitor.Id));

            _jobMonitoringService.AssignHangfireJob(monitor.Id, hangfireJobId);
        }

        public void SyncAcumaticaStockItems(List<long> spids, bool automaticEnable)
        {
            var context = new AcumaticaStockItemImportContext
            {
                ShopifyProductIds = spids,
                IsSyncEnabled = automaticEnable,
                CreateInventoryReceipts = false,
                SynchronizeOnly = true,
                WarehouseId = null,
            };

            var monitor = _jobMonitoringService
                .ProvisionJobMonitor(BackgroundJobType.ImportAcumaticaStockItems, false);

            var hangfireJobId = BackgroundJob.Enqueue<JobRunner>(
                x => x.ImportAcumaticaStockItems(_tenantContext.InstanceId, context, monitor.Id));

            _jobMonitoringService.AssignHangfireJob(monitor.Id, hangfireJobId);
        }


        public void ImportNewShopifyProduct(ShopifyNewProductImportContext context)
        {
            var monitor = _jobMonitoringService
                .ProvisionJobMonitor(BackgroundJobType.ImportNewShopifyProduct, false);

            var hangfireJobId = BackgroundJob.Enqueue<JobRunner>(
                    x => x.ImportNewShopifyProduct(_tenantContext.InstanceId, context, monitor.Id));

            _jobMonitoringService.AssignHangfireJob(monitor.Id, hangfireJobId);
        }

        public void ImportAddShopifyVariantsToProduct(ShopifyAddVariantImportContext context)
        {
            var monitor = _jobMonitoringService
                .ProvisionJobMonitor(BackgroundJobType.ImportAddShopifyVariantsToProduct, false);

            var hangfireJobId = BackgroundJob.Enqueue<JobRunner>(
                    x => x.ImportAddShopifyVariantsToProduct(_tenantContext.InstanceId, context, monitor.Id));

            _jobMonitoringService.AssignHangfireJob(monitor.Id, hangfireJobId);
        }

        public void EndToEndSyncStart()
        {
            var monitor = _jobMonitoringService.ProvisionJobMonitor(BackgroundJobType.EndToEndSync, false);

            var hangfireJobId = BackgroundJob.Enqueue<JobRunner>(
                x => x.EndToEndSync(_tenantContext.InstanceId, monitor.Id));

            _jobMonitoringService.AssignHangfireJob(monitor.Id, hangfireJobId);
        }

        public void EndToEndSyncStop()
        {
            var monitor = _jobMonitoringService.RetrieveMonitorByTypeNoTracking(BackgroundJobType.EndToEndSync);
            if (monitor != null)
            {
                _jobMonitoringService.SendKillSignal(monitor.Id);
                _executionLogService.Log($"End-to-End Sync - recurring job - stop signal received");
            }
        }
    }
}

