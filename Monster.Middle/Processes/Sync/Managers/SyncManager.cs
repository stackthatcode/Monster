using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using Autofac;
using Monster.Acumatica.Http;
using Monster.Middle.Persist.Multitenant;
using Monster.Middle.Processes.Sync.Inventory.Model;
using Monster.Middle.Processes.Sync.Inventory.Workers;
using Monster.Middle.Processes.Sync.Orders.Workers;
using Monster.Middle.Security;
using Push.Foundation.Utilities.Logging;

namespace Monster.Middle.Processes.Sync.Managers
{
    public class SyncManager
    {
        private readonly ILifetimeScope _lifetimeScope;

        private readonly PreferencesRepository _preferencesRepository;
        private readonly ConnectionContext _connectionContext;
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
                ConnectionContext connectionContext,
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
            _lifetimeScope = lifetimeScope;
            _logger = logger;
            _warehouseLocationSync = warehouseLocationSync;
            _shopifyInventorySync = shopifyInventorySync;
        }


        public void RoutineCustomerSync()
        {
            AcumaticaSessionRun(() => _acumaticaCustomerSync.Run());
        }

        public void RoutineOrdersSync()
        {
            const int workerCount = 2;
            _logger.Debug($"Starting AcumaticaOrderSync -> RunParallel with {workerCount} threads");

            ServicePointManager.DefaultConnectionLimit = 100;
            var queue = _acumaticaOrderSync.BuildQueue();

            // This can be abstracted
            var threads = new List<Thread>();
            for (var i = 0; i < workerCount; i++)
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
                    var childConnectionContext = childScope.Resolve<ConnectionContext>();
                    var childAcumaticaContext = childScope.Resolve<AcumaticaHttpContext>();
                    var childOrderSync = childScope.Resolve<AcumaticaOrderSync>();

                    _logger.Debug(
                        $"OrderSyncInChildScope - Acumatica Context: {childAcumaticaContext.ObjectIdentifier}");
                    childConnectionContext.Initialize(instanceId);

                    AcumaticaSessionRun(() => childOrderSync.RunWorker(queue), childAcumaticaContext);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
        }
        
        public void RoutinePaymentSync()
        {
            AcumaticaSessionRun(() => _acumaticaPaymentSync.RunPaymentsForOrders());
        }

        public void RoutineRefundSync()
        {
            AcumaticaSessionRun(() =>
            {
                _acumaticaRefundSync.RunReturns();
                _acumaticaRefundSync.RunCancels();
                _acumaticaPaymentSync.RunPaymentsForRefunds();
            });
        }
        
        public void RoutineFulfillmentSync()
        {
            var preferences = _preferencesRepository.RetrievePreferences();
            var fulfilledInAcumatica = preferences.FulfillmentInAcumatica.Value;

            // Sync Fulfillments to Acumatica Shipments
            if (!fulfilledInAcumatica)
            {
                // TODO - this is vulnerable to failure of any one
                AcumaticaSessionRun(() =>
                {
                    _acumaticaShipmentSync.RunShipments();
                    _acumaticaShipmentSync.RunConfirmShipments();
                    _acumaticaShipmentSync.RunSingleInvoicePerShipmentSalesRef();
                });
            }

            // Sync Shipments to Shopify Fulfillments
            if (fulfilledInAcumatica)
            {
                _shopifyFulfillmentSync.Run();
            }
        }
        

        public void SynchronizeWarehouseLocation()
        {
            _warehouseLocationSync.Run();
        }

        public void ImportIntoAcumatica(AcumaticaInventoryImportContext context)
        {
            AcumaticaSessionRun(() =>
            {
                _acumaticaInventorySync.Run(context);
            });
        }

        public void PushInventoryCountsToShopify()
        {
            _shopifyInventorySync.Run();
        }
        
        public void SingleOrderPush(long shopifyOrderId)
        {
            AcumaticaSessionRun(() =>
            {
                _acumaticaOrderSync.RunOrder(shopifyOrderId);
            });
        }



        public void AcumaticaSessionRun(Action action)
        {
            AcumaticaSessionRun(action, _acumaticaContext);
        }

        public void AcumaticaSessionRun(Action action, AcumaticaHttpContext context)
        {
            try
            {
                context.Login();
                action();
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
            finally
            {
                if (context.IsLoggedIn)
                {
                    context.Logout();
                }
            }
        }
    }
}

