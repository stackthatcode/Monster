using System;
using Hangfire;
using Monster.Middle.Persist.Multitenant;
using Monster.Middle.Persist.Multitenant.Model;
using Monster.Middle.Services;
using Push.Foundation.Utilities.Logging;

namespace Monster.Middle.Directors
{
    public class QueuingService
    {
        private readonly TenantContext _tenantContext;
        private readonly TenantRepository _tenantRepository;
        private readonly JobRepository _jobRepository;
        private readonly IPushLogger _logger;

        private readonly object _lock001 = new object();
        private readonly object _lock002 = new object();
        private readonly object _lock003 = new object();


        public QueuingService(
                TenantContext tenantContext, 
                TenantRepository tenantRepository, 
                JobRepository jobRepository,
                IPushLogger logger)
        {
            _tenantContext = tenantContext;
            _tenantRepository = tenantRepository;
            _jobRepository = jobRepository;
            _logger = logger;
        }


        public void SyncWarehouseAndLocation()
        {
            lock (_lock001)
            {
                if (_jobRepository.PendingExists(QueuedJobType.SyncWarehouseAndLocation))
                {
                    _logger.Info("SyncWarehouseAndLocation already running");
                    return;
                }

                var tenantId = _tenantContext.InstallationId;
                var jobId = BackgroundJob
                    .Enqueue<SyncDirector>(x => x.SyncWarehouseAndLocation(tenantId));

                _jobRepository.SetPending(QueuedJobType.SyncWarehouseAndLocation, jobId);
            }
        }

        public void LoadInventoryIntoAcumatica()
        {
            lock (_lock002)
            {
                if (_jobRepository.PendingExists(QueuedJobType.LoadInventoryIntoAcumatica))
                {
                    _logger.Info("LoadAcumaticaInventory already running");
                    return;
                }

                var tenantId = _tenantContext.InstallationId;
                var jobId = BackgroundJob.Enqueue<SyncDirector>(
                    x => x.LoadInventoryIntoAcumatica(tenantId));

                _jobRepository.SetPending(QueuedJobType.LoadInventoryIntoAcumatica, jobId);
            }
        }
        
        public void LoadInventoryIntoShopify()
        {
            lock (_lock003)
            {
                if (_jobRepository.PendingExists(QueuedJobType.LoadInventoryIntoShopify))
                {
                    _logger.Info("LoadInventoryIntoShopify already running");
                    return;
                }

                var tenantId = _tenantContext.InstallationId;

                var jobId = BackgroundJob
                        .Enqueue<SyncDirector>(
                            x => x.LoadInventoryIntoShopify(tenantId));

                _jobRepository.SetPending(QueuedJobType.LoadInventoryIntoShopify, jobId);
            }
        }


        // Recurring Job - separate category
        public void ScheduleRoutineSync()
        {
            var recurringJobId
                = "RoutineSync:" + _tenantContext.InstallationId;

            RecurringJob.AddOrUpdate<SyncDirector>(
                recurringJobId,
                x => x.RoutineSynchHarness(_tenantContext.InstallationId),
                "*/5 * * * *",
                TimeZoneInfo.Utc);
        }
    }
}

