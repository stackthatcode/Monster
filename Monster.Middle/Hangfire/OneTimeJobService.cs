﻿using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Hangfire;
using Monster.Middle.Persist.Master;
using Monster.Middle.Processes.Sync.Model.Inventory;
using Monster.Middle.Processes.Sync.Services;
using Push.Foundation.Utilities.Logging;


namespace Monster.Middle.Hangfire
{
    public class OneTimeJobService
    {
        private readonly ConnectionContext _tenantContext;
        private readonly SystemStateRepository _stateRepository;


        public OneTimeJobService(
            ConnectionContext tenantContext,
            SystemStateRepository stateRepository)
        {
            _tenantContext = tenantContext;
            _stateRepository = stateRepository;
        }


        public void ConnectToAcumatica()
        {
            QueueJob(BackgroundJobType.ConnectToAcumatica,
                x => x.RunConnectToAcumatica(_tenantContext.InstanceId));
        }

        public void PullAcumaticaRefData()
        {
            QueueJob(BackgroundJobType.PullAcumaticaRefData, 
                x => x.RunPullAcumaticaRefData(_tenantContext.InstanceId));
        }

        public void SyncWarehouseAndLocation()
        {
            QueueJob(BackgroundJobType.SyncWarehouseAndLocation,
                x => x.RunSyncWarehouseAndLocation(_tenantContext.InstanceId));
        }
    
        public void RunDiagnostics()
        {
            QueueJob(BackgroundJobType.Diagnostics, 
                x => x.RunDiagnostics(_tenantContext.InstanceId));
        }

        public void PullInventory()
        {
            QueueJob(BackgroundJobType.PullInventory, 
                x => x.PullInventory(_tenantContext.InstanceId));
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

            QueueJob(BackgroundJobType.ImportIntoAcumatica,
                x => x.ImportIntoAcumatica(_tenantContext.InstanceId, context));
        }
        


        // Worker methods
        //
        private void QueueJob(int jobType, Expression<Action<JobRunner>> action)
        {
            using (var transaction = _stateRepository.BeginTransaction())
            {
                var jobId = BackgroundJob.Enqueue<JobRunner>(action);
                _stateRepository.InsertBackgroundJob(jobType, jobId);
                transaction.Commit();
            }
        }
    }
}

