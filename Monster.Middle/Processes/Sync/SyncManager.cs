using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using Autofac;
using Monster.Acumatica.Http;
using Monster.Middle.Misc.Logging;
using Monster.Middle.Persist.Instance;
using Monster.Middle.Processes.Sync.Model.Inventory;
using Monster.Middle.Processes.Sync.Persist;
using Monster.Middle.Processes.Sync.Workers;
using Push.Foundation.Utilities.Logging;

namespace Monster.Middle.Processes.Sync.Managers
{
    public class SyncManager
    {
        private readonly ILifetimeScope _lifetimeScope;

        private readonly SettingsRepository _settingsRepository;
        private readonly InstanceContext _connectionContext;
        private readonly ExecutionLogService _executionLogService;
        private readonly AcumaticaHttpContext _acumaticaContext;

        private readonly WarehouseLocationPut _warehouseLocationSync;
        private readonly ShopifyInventoryPut _shopifyInventorySync;
        private readonly ShopifyFulfillmentPut _shopifyFulfillmentSync;

        private readonly AcumaticaCustomerPut _acumaticaCustomerSync;
        private readonly AcumaticaOrderPut _acumaticaOrderSync;
        private readonly AcumaticaInventoryPut _acumaticaInventorySync;
        private readonly AcumaticaOrderPaymentPut _acumaticaPaymentSync;
        private readonly AcumaticaRefundPut _acumaticaRefundSync;
        
        private readonly IPushLogger _logger;


        public SyncManager(
                AcumaticaHttpContext acumaticaContext,
                AcumaticaCustomerPut acumaticaCustomerSync,
                AcumaticaOrderPut acumaticaOrderSync,
                AcumaticaInventoryPut acumaticaInventorySync,
                AcumaticaRefundPut acumaticaRefundSync, 
                AcumaticaOrderPaymentPut acumaticaPaymentSync,

                WarehouseLocationPut warehouseLocationSync,
                ShopifyInventoryPut shopifyInventorySync,
                ShopifyFulfillmentPut shopifyFulfillmentSync,

                SettingsRepository settingsRepository,
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
            
            _shopifyFulfillmentSync = shopifyFulfillmentSync;
            _settingsRepository = settingsRepository;

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
            var preferenece = _settingsRepository.RetrieveSettingss();
            var msg = $"Starting Order Sync with {preferenece.MaxParallelAcumaticaSyncs} worker(s)";
            _executionLogService.Log(msg);

            ServicePointManager.DefaultConnectionLimit = 100;
            var queue = _acumaticaOrderSync.BuildOrderPutQueue();

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
                    var childOrderSync = childScope.Resolve<AcumaticaOrderPut>();

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

