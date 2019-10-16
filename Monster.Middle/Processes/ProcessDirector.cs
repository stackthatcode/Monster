using System;
using System.Linq.Expressions;
using Monster.Middle.Hangfire;
using Monster.Middle.Persist.Instance;
using Monster.Middle.Processes.Acumatica;
using Monster.Middle.Processes.Acumatica.Services;
using Monster.Middle.Processes.Misc;
using Monster.Middle.Processes.Shopify.Workers;
using Monster.Middle.Processes.Sync.Model.Inventory;
using Monster.Middle.Processes.Sync.Model.Status;
using Monster.Middle.Processes.Sync.Persist;
using Monster.Middle.Processes.Sync.Services;
using Monster.Middle.Processes.Sync.Status;
using Push.Foundation.Utilities.Logging;


namespace Monster.Middle.Processes.Sync.Managers
{
    public class ProcessDirector
    {
        private readonly ExternalServiceRepository _connectionRepository;
        private readonly StateRepository _stateRepository;
        private readonly ReferenceDataService _referenceDataService;
        private readonly AcumaticaManager _acumaticaManager;
        private readonly ShopifyManager _shopifyManager;
        private readonly ConfigStatusService _configStatusService;
        private readonly SyncManager _syncManager;
        private readonly ExecutionLogService _executionLogService;
        private readonly PreferencesRepository _preferencesRepository;
        private readonly ExclusiveJobMonitoringService _monitoringService;
        private readonly IPushLogger _logger;


        public ProcessDirector(
                ExternalServiceRepository connectionRepository,
                StateRepository stateRepository,

                AcumaticaManager acumaticaManager, 
                ShopifyManager shopifyManager, 
                SyncManager syncManager,

                ExecutionLogService executionLogService,
                ConfigStatusService configStatusService,
                ReferenceDataService referenceDataService,
                PreferencesRepository preferencesRepository,
                ExclusiveJobMonitoringService monitoringService,
                IPushLogger logger)
        {
            _connectionRepository = connectionRepository;
            _stateRepository = stateRepository;

            _acumaticaManager = acumaticaManager;
            _shopifyManager = shopifyManager;            
            _syncManager = syncManager;

            _executionLogService = executionLogService;
            _configStatusService = configStatusService;
            _preferencesRepository = preferencesRepository;
            _referenceDataService = referenceDataService;
            _logger = logger;
            _monitoringService = monitoringService;
        }


        // Configuration Jobs
        //
        public void ConnectToShopify()
        {
            try
            {
                _executionLogService.InsertExecutionLog("Testing Shopify Connection");
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
                _executionLogService.InsertExecutionLog("Testing Acumatica Connection");
                _acumaticaManager.TestConnection();
                _stateRepository.UpdateSystemState(
                        x => x.AcumaticaConnState, StateCode.Ok);
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                _stateRepository.UpdateSystemState(
                        x => x.AcumaticaConnState, StateCode.SystemFault);
            }
        }

        public void RefreshAcumaticaRefData()
        {
            try
            {
                _executionLogService.InsertExecutionLog("Pulling Acumatica Reference Data");
                _acumaticaManager.PullReferenceData();
                
                // Update the Reference Data State
                //
                _stateRepository.UpdateSystemState(
                        x => x.AcumaticaRefDataState, StateCode.Ok);

                // Update the Preferences State
                //
                var preferences = _preferencesRepository.RetrievePreferences();
                _referenceDataService.FilterPreferencesAgainstRefData(preferences);
                _connectionRepository.SaveChanges();

                var state = preferences.AreValid() ? StateCode.Ok : StateCode.Invalid;
                _stateRepository.UpdateSystemState(x => x.PreferenceState, state);
            }
            catch (Exception ex)
            {
                _logger.Error(ex);

                _stateRepository.UpdateSystemState(
                        x => x.AcumaticaRefDataState, StateCode.SystemFault);
            }
        }

        public void SyncWarehouseAndLocation()
        {
            try
            {
                _executionLogService.InsertExecutionLog("Pulling Acumatica Warehouses and Shopify Locations");

                // Step 1 - Pull Locations and Warehouses
                _acumaticaManager.PullWarehouses();
                _shopifyManager.PullLocations();

                // Step 2 - Synchronize Locations and Warehouses
                _syncManager.SynchronizeWarehouseLocation();

                // Step 3 - Determine resultant System State
                _configStatusService.UpdateWarehouseSyncStatus();
            }
            catch (Exception ex)
            {
                _logger.Error(ex);

                _stateRepository.UpdateSystemState(
                        x => x.WarehouseSyncState, StateCode.SystemFault);
            }
        }

        public void RunDiagnostics()
        {
            ConnectToShopify();
            RefreshAcumaticaRefData();
            SyncWarehouseAndLocation();
        }



        // Inventory Jobs 
        //
        public void RefreshInventory()
        {
            try
            {
                _executionLogService.InsertExecutionLog("Inventory Refresh - encountered error");

                _shopifyManager.PullInventory();
                _acumaticaManager.PullInventory();
                
                _stateRepository.UpdateSystemState(
                    x => x.InventoryRefreshState, StateCode.Ok);
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                _executionLogService.InsertExecutionLog("Inventory Refresh - encountered error");

                _stateRepository.UpdateSystemState(
                    x => x.InventoryRefreshState, StateCode.SystemFault);
            }
        }
        
        public void ImportInventoryToAcumatica(AcumaticaInventoryImportContext context)
        {
            RefreshInventory();

            var state = _stateRepository.RetrieveSystemStateNoTracking();
            if (state.InventoryRefreshState != StateCode.Ok)
            {
                var msg = "Inventory Refresh is broken; aborting Inventory Import";
                _executionLogService.InsertExecutionLog(msg);
                return;
            }

            _syncManager.ImportIntoAcumatica(context);
        }



        // Synchronization
        //
        public void EndToEndSync()
        {
            _executionLogService.InsertExecutionLog("End-to-End - process starting");

            EndToEndRunner(
                new Action[] 
                {
                    () => _shopifyManager.PullCustomers(),
                    () => _shopifyManager.PullOrders(),
                    () => _shopifyManager.PullTransactions(),
                    () => _acumaticaManager.PullOrdersCustomerShipments()
                },
                x => x.OrderCustomersTransPullState,
                "End-to-End - Customers, Orders, Transactions and Shipments Pull");

            EndToEndRunner(
                new Action[]
                {
                    () => _shopifyManager.PullInventory(),
                    () => _acumaticaManager.PullInventory(),
                },
                x => x.InventoryRefreshState, 
                "End-to-End - Refresh Inventory (Pull)");

            var preferences = _preferencesRepository.RetrievePreferences();


            if (preferences.SyncOrdersEnabled
                    && _stateRepository.CheckSystemState(x => x.CanSyncOrdersToAcumatica()))
            {
                EndToEndRunner(
                    new Action[]
                    {
                        () => _syncManager.SyncCustomersToAcumatica(),
                        () => _syncManager.SyncOrdersToAcumatica(),
                        () => _syncManager.SyncPaymentsToAcumatica(),
                    },
                    x => x.SyncOrdersState,
                    "End-to-End - Sync Customers, Orders, Payments to Acumatica");
            }

            if (preferences.SyncRefundsEnabled
                    && _stateRepository.CheckSystemState(x => x.CanSyncRefundsToAcumatica()))
            {
                EndToEndRunner(
                    new Action[] { () => _syncManager.SyncRefundsToAcumatica() },
                    x => x.SyncRefundsState,
                    "End-to-End - Sync Refunds to Acumatica");
            }

            if (preferences.SyncFulfillmentsEnabled
                    && _stateRepository.CheckSystemState(x => x.CanSyncFulfillmentsToShopify()))
            {
                EndToEndRunner(
                    new Action[] { () => _syncManager.SyncFulfillmentsToShopify() },
                    x => x.SyncFulfillmentsState,
                    "End-to-End - Sync Fulfillments to Shopify");
            }

            if (preferences.SyncInventoryEnabled
                    && _stateRepository.CheckSystemState(x => x.CanSyncInventoryCountsToShopify()))
            {
                EndToEndRunner(
                    new Action[] { () => _syncManager.SyncInventoryCountsToShopify() },
                    x => x.SyncInventoryCountState,
                    "End-to-End - Sync Inventory Count to Shopify");
            }

            _executionLogService.InsertExecutionLog("End-to-End - process finishing");
        }



        private void EndToEndRunner(
                    Action[] actions, Expression<Func<SystemState, int>> stateVariable, string name)
        {
            foreach (var action in actions)
            {
                try
                {
                    if (!_monitoringService.IsEndToEndSyncRunning())
                    {
                        _executionLogService.InsertExecutionLog($"{name} - interrupting");
                        return;
                    }

                    action();
                }
                catch (Exception ex)
                {
                    _stateRepository.UpdateSystemState(stateVariable, StateCode.SystemFault);
                    _executionLogService.InsertExecutionLog($"{name} - encountered an error");
                    _logger.Error(ex);
                    return;
                }
            }

            _stateRepository.UpdateSystemState(stateVariable, StateCode.Ok);
        }
    }
}

