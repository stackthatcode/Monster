using System;
using Hangfire;
using Monster.Middle.Persist.Multitenant;
using Monster.Middle.Persist.Multitenant.Model;
using Monster.Middle.Services;
using Push.Foundation.Utilities.Logging;

namespace Monster.Middle.Jobs
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
                JobRepository jobRepository,
                TenantRepository tenantRepository,
                IPushLogger logger)
        {
            _tenantContext = tenantContext;
            _jobRepository = jobRepository;
            _tenantRepository = tenantRepository;
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
                    .Enqueue<JobRunner>(x => x.RunSyncWarehouseAndLocation(tenantId));

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
                var jobId = BackgroundJob.Enqueue<JobRunner>(
                    x => x.RunLoadInventoryIntoAcumatica(tenantId));

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
                        .Enqueue<JobRunner>(
                            x => x.RunLoadInventoryIntoShopify(tenantId));

                _jobRepository.SetPending(QueuedJobType.LoadInventoryIntoShopify, jobId);
            }
        }


        public string RoutineSyncJobId()
        {
            return "RoutineSync:" + _tenantContext.InstallationId;
        }

        // Recurring Job - separate category
        public void ScheduleRoutineSync()
        {
            var jobId = RoutineSyncJobId();

            using (var transaction = _tenantRepository.BeginTransaction())
            {
                var preferences = _tenantRepository.RetrievePreferences();

                RecurringJob.AddOrUpdate<JobRunner>(
                    x => x.RunRoutineSync(_tenantContext.InstallationId),
                    "*/1 * * * *",
                    TimeZoneInfo.Utc);

                preferences.RealTimeHangFireJobId = jobId;
                _tenantRepository.Entities.SaveChanges();

                transaction.Commit();
            }
        }
    }
}

