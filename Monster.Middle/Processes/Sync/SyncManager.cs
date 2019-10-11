﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using Autofac;
using Monster.Acumatica.Http;
using Monster.Middle.Persist.Instance;
using Monster.Middle.Processes.Sync.Model.Inventory;
using Monster.Middle.Processes.Sync.Persist;
using Monster.Middle.Processes.Sync.Services;
using Monster.Middle.Processes.Sync.Workers;
using Monster.Middle.Processes.Sync.Workers.Inventory;
using Monster.Middle.Processes.Sync.Workers.Orders;
using Push.Foundation.Utilities.Logging;

namespace Monster.Middle.Processes.Sync.Managers
{
    public class SyncManager
    {
        private readonly ILifetimeScope _lifetimeScope;

        private readonly PreferencesRepository _preferencesRepository;
        private readonly InstanceContext _connectionContext;
        private readonly ExecutionLogService _executionLogService;
        private readonly AcumaticaHttpContext _acumaticaContext;

        private readonly WarehouseLocationSync _warehouseLocationSync;
        private readonly ShopifyInventorySync _shopifyInventorySync;
        private readonly ShopifyFulfillmentSync _shopifyFulfillmentSync;

        private readonly AcumaticaCustomerSync _acumaticaCustomerSync;
        private readonly AcumaticaOrderSync _acumaticaOrderSync;
        private readonly AcumaticaShipmentSync _acumaticaShipmentSync;
        private readonly AcumaticaInventorySync _acumaticaInventorySync;
        private readonly AcumaticaOrderPaymentSync _acumaticaPaymentSync;
        private readonly AcumaticaRefundSync _acumaticaRefundSync;
        
        private readonly IPushLogger _logger;


        public SyncManager(
                AcumaticaHttpContext acumaticaContext,
                AcumaticaCustomerSync acumaticaCustomerSync,
                AcumaticaOrderSync acumaticaOrderSync,
                AcumaticaShipmentSync acumaticaShipmentSync,
                AcumaticaInventorySync acumaticaInventorySync,
                AcumaticaRefundSync acumaticaRefundSync, 
                AcumaticaOrderPaymentSync acumaticaPaymentSync,

                WarehouseLocationSync warehouseLocationSync,
                ShopifyInventorySync shopifyInventorySync,
                ShopifyFulfillmentSync shopifyFulfillmentSync,

                PreferencesRepository preferencesRepository,
                InstanceContext connectionContext,
                ExecutionLogService executionLogService,
                ILifetimeScope lifetimeScope,
                IPushLogger logger)
        {
            _acumaticaContext = acumaticaContext;
            _acumaticaCustomerSync = acumaticaCustomerSync;
            _acumaticaInventorySync = acumaticaInventorySync;
            _acumaticaRefundSync = acumaticaRefundSync;
            _acumaticaPaymentSync = acumaticaPaymentSync;
            _acumaticaOrderSync = acumaticaOrderSync;
            _acumaticaShipmentSync = acumaticaShipmentSync;

            _shopifyFulfillmentSync = shopifyFulfillmentSync;
            _preferencesRepository = preferencesRepository;

            _connectionContext = connectionContext;
            _executionLogService = executionLogService;
            _lifetimeScope = lifetimeScope;
            _logger = logger;
            _warehouseLocationSync = warehouseLocationSync;
            _shopifyInventorySync = shopifyInventorySync;
        }


        public void SyncCustomersToAcumatica()
        {
            _acumaticaContext.SessionRun(() => _acumaticaCustomerSync.Run());
        }

        public void SyncOrdersToAcumatica()
        {
            var preferenece = _preferencesRepository.RetrievePreferences();
            var msg = $"Starting Order Sync with {preferenece.MaxParallelAcumaticaSyncs} worker(s)";
            _executionLogService.InsertExecutionLog(msg);

            ServicePointManager.DefaultConnectionLimit = 100;
            var queue = _acumaticaOrderSync.BuildQueue();

            // This can be abstracted
            var threads = new List<Thread>();
            for (var i = 0; i < preferenece.MaxParallelAcumaticaSyncs; i++)
            {
                var t = new Thread(() => OrderSyncInChildScope(queue));
                t.Start();
                threads.Add(t);
            }

            foreach (var t in threads)
            {
                t.Join();
            }
        }

        public void OrderSyncInChildScope(ConcurrentQueue<long> queue)
        {
            // NOTE: it's necessary to catch Exceptions, because this is running in its own thread
            try
            {
                var instanceId = _connectionContext.InstanceId;

                using (var childScope = _lifetimeScope.BeginLifetimeScope())
                {
                    var childConnectionContext = childScope.Resolve<InstanceContext>();
                    var childAcumaticaContext = childScope.Resolve<AcumaticaHttpContext>();
                    var childOrderSync = childScope.Resolve<AcumaticaOrderSync>();

                    _logger.Debug(
                        $"OrderSyncInChildScope - Acumatica Context: {childAcumaticaContext.ObjectIdentifier}");
                    childConnectionContext.Initialize(instanceId);
                    childAcumaticaContext.SessionRun(() => childOrderSync.RunWorker(queue));
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
        }
        
        public void SyncPaymentsToAcumatica()
        {
            _acumaticaContext.SessionRun(() => _acumaticaPaymentSync.RunPaymentsForOrders());
        }

        public void SyncRefundsToAcumatica()
        {
            _acumaticaContext.SessionRun(() =>
            {
                _acumaticaRefundSync.RunReturns();
                _acumaticaRefundSync.RunCancels();
                _acumaticaPaymentSync.RunPaymentsForRefunds();
            });
        }
        
        public void SyncFulfillmentsToShopify()
        {
            // Sync Shipments to Shopify Fulfillments
            _shopifyFulfillmentSync.Run();
        }
        
        public void SynchronizeWarehouseLocation()
        {
            _warehouseLocationSync.Run();
        }

        public void ImportIntoAcumatica(AcumaticaInventoryImportContext context)
        {
            _acumaticaContext.SessionRun(() =>
            {
                _acumaticaInventorySync.RunImportToAcumatica(context);
            });
        }

        public void SyncInventoryCountsToShopify()
        {
            _shopifyInventorySync.Run();
        }
        
        public void SingleOrderPush(long shopifyOrderId)
        {
            _acumaticaContext.SessionRun(() =>
            {
                _acumaticaOrderSync.RunOrder(shopifyOrderId);
            });
        }
    }
}
