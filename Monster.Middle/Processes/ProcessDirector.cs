using System;
using System.Linq.Expressions;
using Monster.Middle.Misc.External;
using Monster.Middle.Misc.Hangfire;
using Monster.Middle.Misc.Logging;
using Monster.Middle.Misc.State;
using Monster.Middle.Persist.Instance;
using Monster.Middle.Processes.Acumatica;
using Monster.Middle.Processes.Acumatica.Services;
using Monster.Middle.Processes.Shopify;
using Monster.Middle.Processes.Sync.Model.Inventory;
using Monster.Middle.Processes.Sync.Model.Status;
using Monster.Middle.Processes.Sync.Persist;
using Monster.Middle.Processes.Sync.Services;
using Push.Foundation.Utilities.Logging;


namespace Monster.Middle.Processes.Sync.Managers
{
    public class ProcessDirector
    {
        private readonly ExternalServiceRepository _connectionRepository;
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
                ExternalServiceRepository connectionRepository,
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
            _connectionRepository = connectionRepository;
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
            try
            {
                _executionLogService.Log("Testing Shopify Connection");
                _shopifyManager.TestConnection();
                _stateRepository.UpdateSystemState(x => x.ShopifyConnState, StateCode.Ok);
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                _stateRepository.UpdateSystemState(x => x.ShopifyConnState, StateCode.SystemFault);
            }
        }

        public void ConnectToAcumatica()
        {
            try
            {
                _executionLogService.Log("Testing Acumatica Connection");
                _acumaticaManager.TestConnection();
                _stateRepository.UpdateSystemState(x => x.AcumaticaConnState, StateCode.Ok);
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                _stateRepository.UpdateSystemState(
                        x => x.AcumaticaConnState, StateCode.SystemFault);
            }
        }

        public void RefreshReferenceData()
        {
            try
            {
                _executionLogService.Log("Refreshing Acumatica Reference Data");
                _acumaticaManager.PullReferenceData();
                
                // Update the Reference Data State
                //
                _stateRepository.UpdateSystemState(x => x.AcumaticaRefDataState, StateCode.Ok);

                // Reconcile based on any changes
                //
                _combinedRefDataService.ReconcileSettingsWithRefData();
                _combinedRefDataService.ReconcilePaymentGatewaysWithRefData();

                // Retrieve the refreshed Settings
                //
                _configStatusService.RefreshSettingsStatus();
                _configStatusService.RefreshSettingsTaxesStatus();
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                _stateRepository.UpdateSystemState(x => x.AcumaticaRefDataState, StateCode.SystemFault);
            }
        }

        public void SyncWarehouseAndLocation()
        {
            try
            {
                _executionLogService.Log("Refresh Acumatica Warehouses and Shopify Locations");

                // Step 1 - Pull Locations and Warehouses
                _acumaticaManager.PullWarehouses();
                _shopifyManager.PullLocations();

                // Step 2 - Synchronize Locations and Warehouses
                _syncManager.SynchronizeWarehouseLocation();

                // Step 3 - Determine resultant System State
                _configStatusService.RefreshWarehouseSyncStatus();
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                _stateRepository.UpdateSystemState(x => x.WarehouseSyncState, StateCode.SystemFault);
            }
        }

        public void RunDiagnostics()
        {
            ConnectToShopify();
            RefreshReferenceData();
            SyncWarehouseAndLocation();
        }



        // Inventory Jobs 
        //
        public void RefreshInventory()
        {
            try
            {
                _executionLogService.Log("Inventory Refresh - encountered error");

                _shopifyManager.PullInventory();
                _acumaticaManager.PullInventory();
                
                _stateRepository.UpdateSystemState(x => x.InventoryRefreshState, StateCode.Ok);
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                _executionLogService.Log("Inventory Refresh - encountered error");

                _stateRepository.UpdateSystemState(x => x.InventoryRefreshState, StateCode.SystemFault);
            }
        }
        
        public void ImportInventoryToAcumatica(AcumaticaInventoryImportContext context)
        {
            RefreshInventory();

            var state = _stateRepository.RetrieveSystemStateNoTracking();
            if (state.InventoryRefreshState != StateCode.Ok)
            {
                var msg = "Inventory Refresh is broken; aborting Inventory Import";
                _executionLogService.Log(msg);
                return;
            }

            _syncManager.ImportIntoAcumatica(context);
        }



        // Synchronization
        //
        public void EndToEndSync()
        {
            _executionLogService.Log("End-to-End - process starting");

            EndToEndRunner(
                new Action[] 
                {
                    () => _shopifyManager.PullCustomers(),
                    () => _shopifyManager.PullOrders(),
                    () => _shopifyManager.PullTransactions(),
                },
                x => x.ShopifyOrderEtcGetState,
                "End-to-End - Get Customers, Orders, Transactions from Shopify");

            EndToEndRunner(
                new Action[]
                {
                    () => _acumaticaManager.PullOrdersAndCustomer()
                },
                x => x.AcumaticaOrderEtcGetState,
                "End-to-End - Get Orders, Shipments and Customers from Acumatica");

            EndToEndRunner(
                new Action[]
                {
                    () => _shopifyManager.PullInventory(),
                    () => _acumaticaManager.PullInventory(),
                },
                x => x.InventoryRefreshState, 
                "End-to-End - Refresh Inventory from Shopify and Acumatica");

            var settings = _settingsRepository.RetrieveSettings();

            if (settings.SyncOrdersEnabled
                    && _stateRepository.CheckSystemState(x => x.CanSyncOrdersToAcumatica()))
            {
                EndToEndRunner(
                    new Action[]
                    {
                        () => _syncManager.SyncCustomersToAcumatica(),
                        () => _syncManager.SyncOrdersToAcumatica(),
                        () => _syncManager.SyncPaymentsToAcumatica(),
                    },
                    x => x.AcumaticaOrderEtcPutState, 
                    "End-to-End - Sync Customers, Orders, Payments to Acumatica");
            }

            if (settings.SyncRefundsEnabled
                    && _stateRepository.CheckSystemState(x => x.CanSyncRefundsToAcumatica()))
            {
                EndToEndRunner(
                    new Action[] { () => _syncManager.SyncRefundsToAcumatica() },
                    x => x.AcumaticaRefundPutState, 
                    "End-to-End - Sync Refunds to Acumatica");
            }

            if (settings.SyncFulfillmentsEnabled
                    && _stateRepository.CheckSystemState(x => x.CanSyncFulfillmentsToShopify()))
            {
                EndToEndRunner(
                    new Action[] { () => _syncManager.SyncFulfillmentsToShopify() },
                    x => x.ShopifyFulfillmentPutState, 
                    "End-to-End - Sync Fulfillments to Shopify");
            }

            if (settings.SyncInventoryEnabled
                    && _stateRepository.CheckSystemState(x => x.CanSyncInventoryCountsToShopify()))
            {
                EndToEndRunner(
                    new Action[] { () => _syncManager.SyncInventoryCountsToShopify() },
                    x => x.ShopifyInventoryPutState, 
                    "End-to-End - Sync Inventory Count to Shopify");
            }

            _executionLogService.Log("End-to-End - process finishing");
        }


        private void EndToEndRunner(
                Action[] actions, Expression<Func<SystemState, int>> stateVariable, string descriptor)
        {
            foreach (var action in actions)
            {
                try
                {
                    var monitor = _monitoringService.RetrieveMonitorByType(BackgroundJobType.EndToEndSync);
                    if (monitor == null || monitor.ReceivedKillSignal)
                    {
                        _executionLogService.Log($"{descriptor} - execution has been interrupted");
                        return;
                    }

                    action();
                }
                catch (Exception ex)
                {

                    _stateRepository.UpdateSystemState(stateVariable, StateCode.SystemFault);
                    _executionLogService.Log($"{descriptor} - encountered an error");
                    _logger.Error(ex);
                    return;
                }
            }

            _stateRepository.UpdateSystemState(stateVariable, StateCode.Ok);
        }
    }
}

