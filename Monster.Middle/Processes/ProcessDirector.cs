using System;
using System.Linq.Expressions;
using Monster.Middle.Misc.Hangfire;
using Monster.Middle.Misc.Logging;
using Monster.Middle.Misc.State;
using Monster.Middle.Persist.Instance;
using Monster.Middle.Processes.Acumatica;
using Monster.Middle.Processes.Acumatica.Services;
using Monster.Middle.Processes.Shopify;
using Monster.Middle.Processes.Sync.Model.Inventory;
using Monster.Middle.Processes.Sync.Persist;
using Monster.Middle.Processes.Sync.Services;
using Push.Foundation.Utilities.Logging;


namespace Monster.Middle.Processes.Sync.Managers
{
    public class ProcessDirector
    {
        private readonly StateRepository _stateRepository;
        private readonly CombinedRefDataService _combinedRefDataService;
        private readonly AcumaticaManager _acumaticaManager;
        private readonly ShopifyManager _shopifyManager;
        private readonly ConfigStatusService _configStatusService;
        private readonly SyncManager _syncManager;
        private readonly ExecutionLogService _executionLogService;
        private readonly SettingsRepository _settingsRepository;
        private readonly JobMonitoringService _monitoringService;
        private readonly IPushLogger _logger;


        public ProcessDirector(
                StateRepository stateRepository,

                AcumaticaManager acumaticaManager, 
                ShopifyManager shopifyManager, 
                SyncManager syncManager,

                ExecutionLogService executionLogService,
                ConfigStatusService configStatusService,
                CombinedRefDataService combinedRefDataService,
                SettingsRepository settingsRepository,
                JobMonitoringService monitoringService,
                IPushLogger logger)
        {
            _stateRepository = stateRepository;

            _acumaticaManager = acumaticaManager;
            _shopifyManager = shopifyManager;            
            _syncManager = syncManager;

            _executionLogService = executionLogService;
            _configStatusService = configStatusService;
            _settingsRepository = settingsRepository;
            _combinedRefDataService = combinedRefDataService;
            _logger = logger;
            _monitoringService = monitoringService;
        }


        // Configuration Jobs
        //
        public void ConnectToShopify()
        {
            Run(() => _shopifyManager.TestConnection(), x => x.ShopifyConnState);
        }

        public void ConnectToAcumatica()
        {
            Run(() => _acumaticaManager.TestConnection(), x => x.AcumaticaConnState);
        }

        public void RefreshReferenceData()
        {
            Run(new Action[]
            {
                () => _acumaticaManager.PullReferenceData(),
                () => _shopifyManager.PullReferenceData(),
                () => _combinedRefDataService.ReconcileSettingsWithRefData(),
                () => _combinedRefDataService.ReconcilePaymentGatewaysWithRefData(),
                () => _configStatusService.RefreshSettingsStatus(),
                () => _configStatusService.RefreshSettingsTaxesStatus(),
            }, 
            x => x.AcumaticaRefDataState);
        }


        public void SyncWarehouseAndLocation()
        {
            Run(new Action[]
                {
                    () => _acumaticaManager.PullWarehouses(),
                    () => _shopifyManager.PullLocations(),
                    () => _syncManager.SynchronizeWarehouseLocation(),
                    () => _configStatusService.RefreshWarehouseSyncStatus(),
                },
                x => x.WarehouseSyncState);
        }


        public void RunDiagnostics()
        {
            Run(() =>
            {
                _shopifyManager.TestConnection();
                RefreshReferenceData();
                SyncWarehouseAndLocation();
            });
        }


        // Inventory Jobs 
        //
        public void RefreshInventory()
        {
            Run(() => RefreshInventoryWorker(), x => x.InventoryRefreshState);
        }

        private void RefreshInventoryWorker()
        {
            _shopifyManager.PullInventory();
            _acumaticaManager.PullInventory();
        }


        public void ImportAcumaticaStockItems(AcumaticaStockItemImportContext context)
        {
            Action action = () =>
            {
                RefreshInventory();

                var state = _stateRepository.RetrieveSystemStateNoTracking();
                if (state.InventoryRefreshState != StateCode.Ok)
                {
                    _executionLogService.Log("Inventory Refresh is broken; aborting ImportAcumaticaStockItems");
                    return;
                }

                _syncManager.ImportAcumaticaStockItems(context);
            };

            Run(action);
        }

        public void ImportNewShopifyProduct(ShopifyNewProductImportContext context)
        {
            Action action = () =>
            {
                RefreshInventory();
                var state = _stateRepository.RetrieveSystemStateNoTracking();
                if (state.InventoryRefreshState != StateCode.Ok)
                {
                    _executionLogService.Log("Inventory Refresh is broken; aborting ImportNewShopifyProduct");
                    return;
                }

                _syncManager.ImportNewShopifyProduct(context);
            };

            Run(action);
        }

        public void ImportAddShopifyVariantsToProduct(ShopifyAddVariantImportContext context)
        {
            Action action = () =>
            {

                RefreshInventory();

                var state = _stateRepository.RetrieveSystemStateNoTracking();
                if (state.InventoryRefreshState != StateCode.Ok)
                {
                    _executionLogService.Log(
                        "Inventory Refresh is broken; aborting ImportAddShopifyVariantsToProduct");

                    return;
                }

                _syncManager.ImportAddShopifyVariantsToProduct(context);
            };

            Run(action);
        }


        public void EndToEndSyncSingleOrder(long shopifyOrderId)
        {
            var settings = _settingsRepository.RetrieveSettings();

            if (settings.PullFromShopifyEnabled)
            {
                RunPullFromShopify();
            }

            if (settings.PullFromAcumaticaEnabled)
            {
                RunPullFromAcumatica();
            }

            _syncManager.SyncSingleOrderToAcumatica(shopifyOrderId);

            _syncManager.SyncSingleOrderFulfillmentsToShopify(shopifyOrderId);
        }

        public void EndToEndSync()
        {
            var settings = _settingsRepository.RetrieveSettings();

            if (settings.PullFromShopifyEnabled)
            {
                RunPullFromShopify();
            }

            if (settings.PullFromAcumaticaEnabled)
            {
                RunPullFromAcumatica();
            }

            if (settings.SyncFulfillmentsEnabled
                && _stateRepository.CheckSystemState(x => x.CanSyncFulfillmentsToShopify()))
            {
                Run(new Action[]
                {
                    () => _syncManager.SyncFulfillmentsToShopify()
                });
            }
            
            if (settings.SyncOrdersEnabled)
            {
                Run(new Action[]
                    {
                        () => _syncManager.SyncCustomersToAcumatica(),
                        () => _syncManager.SyncOrdersToAcumatica(),
                        () => _syncManager.SyncPaymentsToAcumatica(),
                    });
            }

            if (settings.SyncInventoryEnabled
                    && _stateRepository.CheckSystemState(x => x.CanSyncInventoryCountsToShopify()))
            {

                Run(new Action[]
                    {
                        () => _shopifyManager.PullInventory(),
                        () => _acumaticaManager.PullInventory(),
                        () => _syncManager.SyncInventoryCountsToShopify()
                    });
            }
        }

        private void RunPullFromAcumatica()
        {
            Run(new Action[]
            {
                () => _acumaticaManager.PullOrdersAndCustomer()
            });
        }

        private void RunPullFromShopify()
        {
            Run(new Action[]
            {
                () => _shopifyManager.PullCustomers(),
                () => _shopifyManager.PullOrders(),
                () => _shopifyManager.PullTransactions(),
            });
        }


        private void Run(Action action, Expression<Func<SystemState, int>> stateProperty = null)
        {
            Run(new [] { action }, stateProperty);
        }

        private void Run(Action[] actions, Expression<Func<SystemState, int>> stateProperty = null)
        {
            try
            {
                foreach (var action in actions)
                {
                    if (_monitoringService.DetectCurrentJobInterrupt())
                    {
                        return;
                    }

                    action();
                }

                if (stateProperty != null)
                {
                    _stateRepository.UpdateSystemState(stateProperty, StateCode.Ok);
                }
            }
            catch
            {
                if (stateProperty != null)
                {
                    _stateRepository.UpdateSystemState(stateProperty, StateCode.SystemFault);
                }

                // The Job Runner Logs errors
                //
                throw;
            }
        }
    }
}

