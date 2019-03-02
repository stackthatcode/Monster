using System;
using System.Linq.Expressions;
using Monster.Middle.Hangfire;
using Monster.Middle.Persist.Multitenant;
using Monster.Middle.Processes.Acumatica;
using Monster.Middle.Processes.Acumatica.Persist;
using Monster.Middle.Processes.Shopify;
using Monster.Middle.Processes.Shopify.Persist;
using Monster.Middle.Processes.Sync.Inventory;
using Monster.Middle.Processes.Sync.Inventory.Model;
using Monster.Middle.Processes.Sync.Orders;
using Monster.Middle.Processes.Sync.Status;
using Push.Foundation.Utilities.Logging;


namespace Monster.Middle.Processes.Sync
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
        private readonly InventoryManager _inventoryManager;
        private readonly StatusService _inventoryStatusService;
        private readonly OrderManager _orderManager;
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
                InventoryManager inventoryManager,
                OrderManager orderManager,
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
            _inventoryManager = inventoryManager;
            _inventoryStatusService = inventoryStatusService;

            _orderManager = orderManager;
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
                _stateRepository
                    .UpdateSystemState(x => x.ShopifyConnection, SystemState.Ok);
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                _stateRepository
                    .UpdateSystemState(x => x.ShopifyConnection, SystemState.SystemFault);
            }
        }

        public void ConnectToAcumatica()
        {
            try
            {
                _acumaticaManager.TestConnection();
                _stateRepository
                    .UpdateSystemState(
                        x => x.AcumaticaConnection, SystemState.Ok);
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                _stateRepository
                    .UpdateSystemState(
                        x => x.AcumaticaConnection, SystemState.SystemFault);
            }
        }

        public void PullAcumaticaReferenceData()
        {
            try
            {
                _acumaticaManager.PullReferenceData();
                
                _stateRepository
                    .UpdateSystemState(x => x.AcumaticaReferenceData, SystemState.Ok);

                var preferences = _preferencesRepository.RetrievePreferences();
                _referenceDataService.FilterPreferencesAgainstRefData(preferences);
                _connectionRepository.SaveChanges();

                var state = preferences.AreValid() ? SystemState.Ok : SystemState.Invalid;

                _stateRepository.UpdateSystemState(x => x.PreferenceSelections, state);
            }
            catch (Exception ex)
            {
                _logger.Error(ex);

                _stateRepository
                    .UpdateSystemState(
                        x => x.AcumaticaReferenceData, SystemState.SystemFault);
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
                _inventoryManager.SynchronizeWarehouseLocation();

                // Step 3 - Determine resultant System State
                _inventoryStatusService.UpdateWarehouseSyncStatus();
            }
            catch (Exception ex)
            {
                _logger.Error(ex);

                _stateRepository.UpdateSystemState(
                        x => x.WarehouseSync, SystemState.SystemFault);
            }
        }

        public void RunDiagnostics()
        {
            ConnectToShopify();
            PullAcumaticaReferenceData();
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
                
                _stateRepository.UpdateSystemState(x => x.InventoryPull, SystemState.Ok);
            }
            catch (Exception ex)
            {
                _logger.Error(ex);

                _stateRepository.UpdateSystemState(
                    x => x.InventoryPull, SystemState.SystemFault);
            }
        }
        
        public void ImportIntoAcumatica(AcumaticaInventoryImportContext context)
        {
            RunImpervious(() => _shopifyManager.PullInventory());
            RunImpervious(() => _inventoryManager.ImportIntoAcumatica(context));
        }


        // Synchronization
        public void FullSync()
        {
            RunImpervious(() => _shopifyManager.PullCustomers());
            RunImpervious(() => _shopifyManager.PullOrders());
            RunImpervious(() => _shopifyManager.PullTransactions());

            RunImpervious(() => _acumaticaManager.PullOrderAndCustomersAndShipments());

            RunImpervious(() => _orderManager.RoutineCustomerSync());
            RunImpervious(() => _orderManager.RoutineOrdersSync());
            RunImpervious(() => _orderManager.RoutinePaymentSync());
            RunImpervious(() => _orderManager.RoutineRefundSync());
            RunImpervious(() => _orderManager.RoutineFulfillmentSync());
            
            RunImpervious(() => _shopifyManager.PullInventory());
            RunImpervious(() => _acumaticaManager.PullInventory());
            RunImpervious(() => _inventoryManager.PushInventoryCountsToShopify());
        }


        // Swallows Exceptions to enable sequences of tasks to be run 
        // ... uninterrupted even if one fails
        private void RunImpervious(Action task)
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

        private void RunImpervious(
                Action task, Expression<Func<UsrSystemState, int>> statePropLambda)
        {
            try
            {
                task();
                _stateRepository.UpdateSystemState(statePropLambda, SystemState.Ok);
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                _stateRepository.UpdateSystemState(statePropLambda, SystemState.SystemFault);
            }
        }
    }
}

