using System;
using System.Linq.Expressions;
using Monster.Middle.Persist.Multitenant;
using Monster.Middle.Processes.Acumatica;
using Monster.Middle.Processes.Acumatica.Persist;
using Monster.Middle.Processes.Acumatica.Workers;
using Monster.Middle.Processes.Shopify;
using Monster.Middle.Processes.Shopify.Persist;
using Monster.Middle.Processes.Shopify.Workers;
using Monster.Middle.Processes.Sync.Inventory.Model;
using Monster.Middle.Processes.Sync.Orders.Workers;
using Monster.Middle.Processes.Sync.Services;
using Push.Foundation.Utilities.Logging;

namespace Monster.Middle.Processes.Sync.Managers
{
    public class SyncDirector
    {
        private readonly ConnectionRepository _connectionRepository;
        private readonly StateRepository _stateRepository;
        private readonly ShopifyBatchRepository _shopifyBatchRepository;
        private readonly AcumaticaBatchRepository _acumaticaBatchRepository;
        private readonly ReferenceDataService _referenceDataService;
        private readonly AcumaticaManager _acumaticaManager;
        private readonly ShopifyManager _shopifyManager;
        private readonly StatusService _inventoryStatusService;
        private readonly SyncManager _syncManager;
        private readonly IPushLogger _logger;
        private readonly PreferencesRepository _preferencesRepository;

        public SyncDirector(
                ConnectionRepository connectionRepository,
                StateRepository stateRepository,
                ShopifyBatchRepository shopifyBatchRepository, 
                AcumaticaBatchRepository acumaticaBatchRepository, 
                AcumaticaManager acumaticaManager, 
                ShopifyManager shopifyManager, 
                StatusService inventoryStatusService,
                SyncManager syncManager,
                ReferenceDataService referenceDataService,
                PreferencesRepository preferencesRepository,
                IPushLogger logger)
        {
            _connectionRepository = connectionRepository;
            _stateRepository = stateRepository;
            _shopifyBatchRepository = shopifyBatchRepository;
            _acumaticaBatchRepository = acumaticaBatchRepository;
            _acumaticaManager = acumaticaManager;
            _shopifyManager = shopifyManager;
            _inventoryStatusService = inventoryStatusService;

            _syncManager = syncManager;
            _logger = logger;
            _preferencesRepository = preferencesRepository;
            _referenceDataService = referenceDataService;
        }
        
        
        // Configuration Jobs
        //
        public void ConnectToShopify()
        {
            try
            {
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

            Run(sequence, false);
        }


        // Swallows Exceptions to enable sequences of tasks to be run 
        // ... uninterrupted even if one fails
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

        public void Run(Action[] actions, bool throwException = true)
        {
            foreach (var action in actions)
            {
                try
                {
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

