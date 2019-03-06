using System;
using Monster.Middle.Persist.Tenant;
using Monster.Middle.Processes.Acumatica.Services;
using Monster.Middle.Processes.Acumatica.Workers;
using Monster.Middle.Processes.Shopify.Workers;
using Monster.Middle.Processes.Sync.Model.Config;
using Monster.Middle.Processes.Sync.Model.Inventory;
using Monster.Middle.Processes.Sync.Model.Misc;
using Monster.Middle.Processes.Sync.Services;
using Monster.Middle.Processes.Sync.Status;
using Push.Foundation.Utilities.Logging;


namespace Monster.Middle.Processes.Sync.Managers
{
    public class SyncDirector
    {
        private readonly ConnectionRepository _connectionRepository;
        private readonly SystemStateRepository _stateRepository;
        private readonly ReferenceDataService _referenceDataService;
        private readonly AcumaticaManager _acumaticaManager;
        private readonly ShopifyManager _shopifyManager;
        private readonly ConfigStatusService _inventoryStatusService;
        private readonly SyncManager _syncManager;
        private readonly ExecutionLogService _executionLogService;
        private readonly IPushLogger _logger;
        private readonly PreferencesRepository _preferencesRepository;

        public SyncDirector(
                ConnectionRepository connectionRepository,
                SystemStateRepository stateRepository,

                AcumaticaManager acumaticaManager, 
                ShopifyManager shopifyManager, 
                SyncManager syncManager,

                ExecutionLogService executionLogService,
                ConfigStatusService inventoryStatusService,
                ReferenceDataService referenceDataService,
                PreferencesRepository preferencesRepository,
                IPushLogger logger)
        {
            _connectionRepository = connectionRepository;
            _stateRepository = stateRepository;

            _acumaticaManager = acumaticaManager;
            _shopifyManager = shopifyManager;            
            _syncManager = syncManager;

            _executionLogService = executionLogService;
            _inventoryStatusService = inventoryStatusService;
            _preferencesRepository = preferencesRepository;
            _referenceDataService = referenceDataService;
            _logger = logger;
        }


        // Configuration Jobs
        //
        public void ConnectToShopify()
        {
            try
            {
                _executionLogService.InsertExecutionLog("Testing Shopify Connection");
                _shopifyManager.TestConnection();
                _stateRepository.UpdateSystemState(x => x.ShopifyConnState, SystemState.Ok);
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                _stateRepository.UpdateSystemState(x => x.ShopifyConnState, SystemState.SystemFault);
            }
        }

        public void ConnectToAcumatica()
        {
            try
            {
                _executionLogService.InsertExecutionLog("Testing Acumatica Connection");
                _acumaticaManager.TestConnection();
                _stateRepository.UpdateSystemState(
                        x => x.AcumaticaConnState, SystemState.Ok);
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                _stateRepository.UpdateSystemState(
                        x => x.AcumaticaConnState, SystemState.SystemFault);
            }
        }

        public void PullAcumaticaRefData()
        {
            try
            {
                _executionLogService.InsertExecutionLog("Pulling Acumatica Reference Data");
                _acumaticaManager.PullReferenceData();
                
                _stateRepository.UpdateSystemState(
                        x => x.AcumaticaRefDataState, SystemState.Ok);

                var preferences = _preferencesRepository.RetrievePreferences();
                _referenceDataService.FilterPreferencesAgainstRefData(preferences);
                _connectionRepository.SaveChanges();

                var state = preferences.AreValid() ? SystemState.Ok : SystemState.Invalid;

                _stateRepository.UpdateSystemState(x => x.PreferenceState, state);
            }
            catch (Exception ex)
            {
                _logger.Error(ex);

                _stateRepository.UpdateSystemState(
                        x => x.AcumaticaRefDataState, SystemState.SystemFault);
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
                _inventoryStatusService.UpdateWarehouseSyncStatus();
            }
            catch (Exception ex)
            {
                _logger.Error(ex);

                _stateRepository.UpdateSystemState(
                        x => x.WarehouseSyncState, SystemState.SystemFault);
            }
        }

        public void RunDiagnostics()
        {
            ConnectToShopify();
            PullAcumaticaRefData();
            SyncWarehouseAndLocation();
        }



        // Inventory Jobs 
        //
        public void PullInventory()
        {
            try
            {
                _shopifyManager.PullInventory();
                _acumaticaManager.PullInventory();
                
                _stateRepository.UpdateSystemState(x => x.InventoryPullState, SystemState.Ok);
            }
            catch (Exception ex)
            {
                _logger.Error(ex);

                _stateRepository.UpdateSystemState(
                        x => x.InventoryPullState, SystemState.SystemFault);
            }
        }
        
        public void ImportIntoAcumatica(AcumaticaInventoryImportContext context)
        {
            Run(() => _shopifyManager.PullInventory());
            Run(() => _syncManager.ImportIntoAcumatica(context));
        }


        // Synchronization
        //
        public void FullSync()
        {
            var sequence = new Action[]
            {
                () => _shopifyManager.PullCustomers(),
                () => _shopifyManager.PullOrders(),
                () => _shopifyManager.PullTransactions(),

                () => _acumaticaManager.PullOrdersAndCustomersAndShipments(),

                () => _syncManager.RoutineCustomerSync(),
                () => _syncManager.RoutineOrdersSync(),
                () => _syncManager.RoutinePaymentSync(),
                () => _syncManager.RoutineRefundSync(),
                () => _syncManager.RoutineFulfillmentSync(),

                () => _shopifyManager.PullInventory(),
                () => _acumaticaManager.PullInventory(),

                () => _syncManager.PushInventoryCountsToShopify()
            };

            RunFullSync(sequence, false);
        }


        // Swallows Exceptions to enable sequences of tasks to be run 
        // ... uninterrupted even if one fails
        //
        private void Run(Action task)
        {
            try
            {
                task();
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
        }

        private void RunFullSync(Action[] actions, bool throwException = true)
        {
            _executionLogService.InsertExecutionLog("Real-Time Sync - Processing ");

            foreach (var action in actions)
            {
                try
                {
                    var state = _stateRepository.RetrieveSystemStateNoTracking();
                    if (!state.IsRealTimeSyncEnabled())
                    {
                        _executionLogService.InsertExecutionLog("Real-Time Sync - Interrupting");
                        return;
                    }

                    action();
                }
                catch (Exception ex)
                {
                    if (throwException)
                    {
                        throw;
                    }

                    _logger.Error(ex);
                }
            }
        }
        
    }
}

