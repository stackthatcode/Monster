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
        private readonly JobMonitoringService _monitoringService;
        private readonly ExecutionLogService _executionLogService;

        public OneTimeJobScheduler(
                InstanceContext tenantContext, 
                JobMonitoringService monitoringService, 
                ExecutionLogService executionLogService)
        {
            _tenantContext = tenantContext;
            _monitoringService = monitoringService;
            _executionLogService = executionLogService;
        }

        public void ConnectToAcumatica()
        {
            var monitor = _monitoringService.ProvisionMonitor(BackgroundJobType.ConnectToAcumatica);
            
            var hangfireJobId = BackgroundJob.Enqueue<JobRunner>(
                    x => x.ConnectToAcumatica(_tenantContext.InstanceId, monitor.Id));

            _monitoringService.AssignHangfireJob(monitor.Id, hangfireJobId);
        }

        public void RefreshAcumaticaReferenceData()
        {
            var monitor = 
                _monitoringService.ProvisionMonitor(BackgroundJobType.RefreshAcumaticaRefData);

            var hangfireJobId = BackgroundJob.Enqueue<JobRunner>(
                    x => x.RefreshAcumaticaRefData(_tenantContext.InstanceId, monitor.Id));

            _monitoringService.AssignHangfireJob(monitor.Id, hangfireJobId);
        }

        public void SyncWarehouseAndLocation()
        {
            var monitor = _monitoringService.ProvisionMonitor(BackgroundJobType.SyncWarehouseAndLocation);

            var hangfireJobId = BackgroundJob.Enqueue<JobRunner>(
                    x => x.SyncWarehouseAndLocation(_tenantContext.InstanceId, monitor.Id));

            _monitoringService.AssignHangfireJob(monitor.Id, hangfireJobId);
        }
        
        public void RunDiagnostics()
        {
            var monitor = _monitoringService.ProvisionMonitor(BackgroundJobType.Diagnostics);

            var hangfireJobId = BackgroundJob.Enqueue<JobRunner>(
                    x => x.Diagnostics(_tenantContext.InstanceId, monitor.Id));

            _monitoringService.AssignHangfireJob(monitor.Id, hangfireJobId);
        }

        public void RefreshInventory()
        {
            var monitor = _monitoringService.ProvisionMonitor(BackgroundJobType.RefreshInventory);

            var hangfireJobId = BackgroundJob.Enqueue<JobRunner>(
                    x => x.RefreshInventory(_tenantContext.InstanceId, monitor.Id));

            _monitoringService.AssignHangfireJob(monitor.Id, hangfireJobId);
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

            var monitor = 
                _monitoringService.ProvisionMonitor(BackgroundJobType.ImportAcumaticaStockItems);

            var hangfireJobId = BackgroundJob.Enqueue<JobRunner>(
                    x => x.ImportAcumaticaStockItems(_tenantContext.InstanceId, context, monitor.Id));

            _monitoringService.AssignHangfireJob(monitor.Id, hangfireJobId);
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

            var monitor = _monitoringService.ProvisionMonitor(BackgroundJobType.ImportAcumaticaStockItems);

            var hangfireJobId = BackgroundJob.Enqueue<JobRunner>(
                x => x.ImportAcumaticaStockItems(_tenantContext.InstanceId, context, monitor.Id));

            _monitoringService.AssignHangfireJob(monitor.Id, hangfireJobId);
        }


        public void ImportNewShopifyProduct(ShopifyNewProductImportContext context)
        {
            var monitor = _monitoringService.ProvisionMonitor(BackgroundJobType.ImportNewShopifyProduct);

            var hangfireJobId = BackgroundJob.Enqueue<JobRunner>(
                    x => x.ImportNewShopifyProduct(_tenantContext.InstanceId, context, monitor.Id));

            _monitoringService.AssignHangfireJob(monitor.Id, hangfireJobId);
        }

        public void ImportAddShopifyVariantsToProduct(ShopifyAddVariantImportContext context)
        {
            var monitor = _monitoringService.ProvisionMonitor(BackgroundJobType.ImportAddShopifyVariantsToProduct);

            var hangfireJobId = BackgroundJob.Enqueue<JobRunner>(
                    x => x.ImportAddShopifyVariantsToProduct(_tenantContext.InstanceId, context, monitor.Id));

            _monitoringService.AssignHangfireJob(monitor.Id, hangfireJobId);
        }

        public void EndToEndSync()
        {
            var monitor = _monitoringService.ProvisionMonitor(BackgroundJobType.EndToEndSync);

            var hangfireJobId = BackgroundJob.Enqueue<JobRunner>(
                x => x.EndToEndSync(_tenantContext.InstanceId, monitor.Id));

            _monitoringService.AssignHangfireJob(monitor.Id, hangfireJobId);
        }

        public void EndToEndSyncSingleShopifyOrder(long shopifyOrderId)
        {
            var monitor = _monitoringService.ProvisionMonitor(BackgroundJobType.EndToEndSync);

            var hangfireJobId = BackgroundJob.Enqueue<JobRunner>(
                x => x.EndToEndSyncSingleOrder(_tenantContext.InstanceId, monitor.Id, shopifyOrderId));

            _monitoringService.AssignHangfireJob(monitor.Id, hangfireJobId);
        }
    }
}

