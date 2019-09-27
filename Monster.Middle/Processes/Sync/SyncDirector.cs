using System;
using System.Collections.Generic;
using Monster.Middle.Hangfire;
using Monster.Middle.Persist.Instance;
using Monster.Middle.Processes.Acumatica.Services;
using Monster.Middle.Processes.Acumatica.Workers;
using Monster.Middle.Processes.Shopify.Workers;
using Monster.Middle.Processes.Sync.Model.Inventory;
using Monster.Middle.Processes.Sync.Model.Misc;
using Monster.Middle.Processes.Sync.Model.Status;
using Monster.Middle.Processes.Sync.Persist;
using Monster.Middle.Processes.Sync.Services;
using Monster.Middle.Processes.Sync.Status;
using Push.Foundation.Utilities.Logging;


namespace Monster.Middle.Processes.Sync.Managers
{
    public class SyncDirector
    {
        private readonly ExternalServiceRepository _connectionRepository;
        private readonly StateRepository _stateRepository;
        private readonly ReferenceDataService _referenceDataService;
        private readonly AcumaticaManager _acumaticaManager;
        private readonly ShopifyManager _shopifyManager;
        private readonly ConfigStatusService _inventoryStatusService;
        private readonly SyncManager _syncManager;
        private readonly ExecutionLogService _executionLogService;
        private readonly PreferencesRepository _preferencesRepository;
        private readonly JobMonitoringService _monitoringService;
        private readonly IPushLogger _logger;


        public SyncDirector(
                ExternalServiceRepository connectionRepository,
                StateRepository stateRepository,

                AcumaticaManager acumaticaManager, 
                ShopifyManager shopifyManager, 
                SyncManager syncManager,

                ExecutionLogService executionLogService,
                ConfigStatusService inventoryStatusService,
                ReferenceDataService referenceDataService,
                PreferencesRepository preferencesRepository,
                JobMonitoringService monitoringService,
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

        public void PullAcumaticaRefData()
        {
            try
            {
                _executionLogService.InsertExecutionLog("Pulling Acumatica Reference Data");
                _acumaticaManager.PullReferenceData();
                
                _stateRepository.UpdateSystemState(
                        x => x.AcumaticaRefDataState, StateCode.Ok);

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
                _inventoryStatusService.UpdateWarehouseSyncStatus();
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
                
                _stateRepository.UpdateSystemState(x => x.InventoryPullState, StateCode.Ok);
            }
            catch (Exception ex)
            {
                _logger.Error(ex);

                _stateRepository.UpdateSystemState(
                        x => x.InventoryPullState, StateCode.SystemFault);
            }
        }
        
        public void ImportIntoAcumatica(AcumaticaInventoryImportContext context)
        {
            Run(() => _shopifyManager.PullInventory());
            Run(() => _syncManager.ImportIntoAcumatica(context));
        }


        // Synchronization
        //
        public void EndToEndSync()
        {
            var preferences = _preferencesRepository.RetrievePreferences();
            var sequence = new List<Action>();

            sequence.AddRange(new Action[] {
                () => _shopifyManager.PullCustomers(),
                () => _shopifyManager.PullOrders(),
                () => _shopifyManager.PullTransactions(),
                () => _acumaticaManager.PullOrdersAndCustomersAndShipments(),
            });

            if (preferences.SyncOrdersEnabled)
            {
                sequence.AddRange(new Action[]
                {
                    () => _syncManager.RoutineCustomerSync(),
                    () => _syncManager.RoutineOrdersSync(),
                    () => _syncManager.RoutinePaymentSync(),
                });
            }

            if (preferences.SyncRefundsEnabled)
            {
                sequence.Add(() => _syncManager.RoutineRefundSync());
            }

            if (preferences.SyncShipmentsEnabled)
            {
                sequence.Add(() => _syncManager.RoutineFulfillmentSync());
            }

            sequence.AddRange(new Action[]
            {
                () => _shopifyManager.PullInventory(),
                () => _acumaticaManager.PullInventory(),
            });

            if (preferences.SyncInventoryEnabled)
            {
                sequence.Add(() => _syncManager.PushInventoryCountsToShopify());
            }

            RunSequence(sequence, false);
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

        private void RunSequence(List<Action> actions, bool throwException = true)
        {
            _executionLogService.InsertExecutionLog("End-to-End Sync - Processing ");

            foreach (var action in actions)
            {
                try
                {
                    var monitor = _monitoringService.GetMonitoringDigest();
                    
                    if (monitor.IsJobTypeActive(BackgroundJobType.EndToEndSync))
                    {
                        _executionLogService.InsertExecutionLog("End-to-End Sync - Interrupting");
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

