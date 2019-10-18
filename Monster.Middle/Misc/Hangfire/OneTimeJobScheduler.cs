using System.Collections.Generic;
using Monster.Middle.Persist.Instance;
using Monster.Middle.Processes.Sync.Model.Inventory;
using BackgroundJob = Hangfire.BackgroundJob;


namespace Monster.Middle.Misc.Hangfire
{
    public class OneTimeJobScheduler
    {
        private readonly InstanceContext _tenantContext;
        private readonly JobMonitoringService _jobMonitoringService;


        public OneTimeJobScheduler(
                InstanceContext tenantContext, JobMonitoringService jobMonitoringService)
        {
            _tenantContext = tenantContext;
            _jobMonitoringService = jobMonitoringService;
        }

        public void ConnectToAcumatica()
        {
            var monitor = _jobMonitoringService.ProvisionJobMonitor(BackgroundJobType.ConnectToAcumatica);
            
            var hangfireJobId = BackgroundJob.Enqueue<JobRunner>(
                    x => x.ConnectToAcumatica(_tenantContext.InstanceId, monitor.Id));

            _jobMonitoringService.AssignHangfireJob(monitor.Id, hangfireJobId);
        }

        public void RefreshAcumaticaReferenceData()
        {
            var monitor = _jobMonitoringService.ProvisionJobMonitor(BackgroundJobType.RefreshAcumaticaRefData);

            var hangfireJobId = BackgroundJob.Enqueue<JobRunner>(
                    x => x.RefreshAcumaticaRefData(_tenantContext.InstanceId, monitor.Id));

            _jobMonitoringService.AssignHangfireJob(monitor.Id, hangfireJobId);
        }

        public void SyncWarehouseAndLocation()
        {
            var monitor = _jobMonitoringService.ProvisionJobMonitor(BackgroundJobType.SyncWarehouseAndLocation);

            var hangfireJobId = BackgroundJob.Enqueue<JobRunner>(
                    x => x.SyncWarehouseAndLocation(_tenantContext.InstanceId, monitor.Id));

            _jobMonitoringService.AssignHangfireJob(monitor.Id, hangfireJobId);
        }
        
        public void RunDiagnostics()
        {
            var monitor = _jobMonitoringService.ProvisionJobMonitor(BackgroundJobType.Diagnostics);

            var hangfireJobId = BackgroundJob.Enqueue<JobRunner>(
                    x => x.Diagnostics(_tenantContext.InstanceId, monitor.Id));

            _jobMonitoringService.AssignHangfireJob(monitor.Id, hangfireJobId);
        }

        public void RefreshInventory()
        {
            var monitor = _jobMonitoringService.ProvisionJobMonitor(BackgroundJobType.RefreshInventory);

            var hangfireJobId = BackgroundJob.Enqueue<JobRunner>(
                    x => x.RefreshInventory(_tenantContext.InstanceId, monitor.Id));

            _jobMonitoringService.AssignHangfireJob(monitor.Id, hangfireJobId);
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

            var monitor = _jobMonitoringService.ProvisionJobMonitor(BackgroundJobType.ImportIntoAcumatica);

            var hangfireJobId = BackgroundJob.Enqueue<JobRunner>(
                x => x.ImportIntoAcumatica(_tenantContext.InstanceId, context, monitor.Id));

            _jobMonitoringService.AssignHangfireJob(monitor.Id, hangfireJobId);
        }


    }
}

