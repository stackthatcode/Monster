﻿using System;
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
        private readonly IPushLogger _logger;


        public QueuingService(
                TenantContext tenantContext, 
                TenantRepository tenantRepository, 
                IPushLogger logger)
        {
            _tenantContext = tenantContext;
            _tenantRepository = tenantRepository;
            _logger = logger;
        }

        public void SyncWarehouseAndLocation()
        {
            var monitor = _tenantRepository.RetrieveJobMonitor();
            if (monitor.WarehouseSyncStatus == JobStatus.Running)
            {
                _logger.Info("SyncWarehouseAndLocation already running");
                return;
            }

            _tenantRepository.UpdateWarehouseSyncStatus(JobStatus.Running);

            var tenantId = _tenantContext.InstallationId;

            BackgroundJob
                .Enqueue<SyncDirector>(
                    x => x.SyncWarehouseAndLocation(tenantId));
        }

        public void LoadInventoryIntoAcumatica()
        {
            var monitor = _tenantRepository.RetrieveJobMonitor();
            if (monitor.LoadInventoryIntoAcumaticaStatus == JobStatus.Running)
            {
                _logger.Info("LoadAcumaticaInventory already running");
                return;
            }

            _tenantRepository
                .UpdateLoadInventoryIntoAcumaticaStatus(JobStatus.Running);

            var tenantId = _tenantContext.InstallationId;

            BackgroundJob
                .Enqueue<SyncDirector>(
                    x => x.LoadInventoryIntoAcumatica(tenantId));
        }

        public void LoadInventoryIntoShopify()
        {
            var monitor = _tenantRepository.RetrieveJobMonitor();
            if (monitor.LoadInventoryIntoShopifyStatus == JobStatus.Running)
            {
                _logger.Info("LoadShopifyInventory already running");
                return;
            }

            _tenantRepository
                .UpdateLoadInventoryIntoShopifyStatus(JobStatus.Running);

            var tenantId = _tenantContext.InstallationId;

            BackgroundJob
                .Enqueue<SyncDirector>(
                    x => x.LoadInventoryIntoShopify(tenantId));
        }

        public void ScheduleRoutineSync()
        {
            var recurringJobId
                = "RoutineSync:" + _tenantContext.InstallationId;

            RecurringJob.AddOrUpdate<SyncDirector>(
                recurringJobId,
                x => x.RoutineSynch(_tenantContext.InstallationId),
                "*/5 * * * *",
                TimeZoneInfo.Utc);
        }
    }
}

