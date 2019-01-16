﻿using System;
using Monster.Middle.Hangfire;
using Monster.Middle.Persist.Multitenant;
using Monster.Middle.Processes.Acumatica;
using Monster.Middle.Processes.Acumatica.Persist;
using Monster.Middle.Processes.Shopify;
using Monster.Middle.Processes.Shopify.Persist;
using Monster.Middle.Processes.Sync.Inventory;
using Monster.Middle.Processes.Sync.Inventory.Model;
using Monster.Middle.Processes.Sync.Inventory.Services;
using Monster.Middle.Processes.Sync.Orders;
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
        private readonly InventorySyncManager _inventorySyncManager;
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
                InventorySyncManager inventorySyncManager,
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
            _inventorySyncManager = inventorySyncManager;
            _inventoryStatusService = inventoryStatusService;

            _orderManager = orderManager;
            _logger = logger;
            _preferencesRepository = preferencesRepository;
            _referenceDataService = referenceDataService;
        }
        
        public void ResetBatchStates()
        {
            _shopifyBatchRepository.Reset();
            _acumaticaBatchRepository.Reset();
        }


        public void ConnectToShopify()
        {
            try
            {
                _shopifyManager.TestConnection();
                _stateRepository
                    .UpdateSystemState(
                        x => x.ShopifyConnection, SystemState.Ok);
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                _stateRepository
                    .UpdateSystemState(
                        x => x.ShopifyConnection, SystemState.SystemFault);
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
                    .UpdateSystemState(
                        x => x.AcumaticaReferenceData, SystemState.Ok);

                var preferences = _preferencesRepository.RetrievePreferences();
                _referenceDataService.FilterPreferencesAgainstRefData(preferences);
                _connectionRepository.SaveChanges();

                var preferencesState =
                        preferences.AreValid() 
                            ? SystemState.Ok : SystemState.Invalid;

                _stateRepository.UpdateSystemState(
                        x => x.PreferenceSelections, preferencesState);
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
                _inventorySyncManager.SynchronizeWarehouseLocation();

                // Step 3 - Determine resultant System State
                var status = _inventoryStatusService.WarehouseSyncStatus();
                var systemState = status.IsOk ? SystemState.Ok : SystemState.Invalid;

                _stateRepository.UpdateSystemState(x => x.WarehouseSync, systemState);
            }
            catch (Exception ex)
            {
                _logger.Error(ex);

                _stateRepository.UpdateSystemState(
                        x => x.WarehouseSync, SystemState.SystemFault);
            }
        }
        
        public void LoadInventoryIntoAcumatica()
        {
            try
            {
                // Step 1 - Pull Shopify Inventory
                _shopifyManager.PullInventory();
                
                // Step 2 - Pull Acumatica Inventory
                _acumaticaManager.PullInventory();

                // Step 3 - Load Shopify Inventory into Acumatica as baseline
                _inventorySyncManager.PushInventoryIntoAcumatica();

                _stateRepository.UpdateSystemState(
                    x => x.AcumaticaInventoryPush, SystemState.Ok);
            }
            catch (Exception ex)
            {
                _logger.Error(ex);

                _stateRepository.UpdateSystemState(
                    x => x.AcumaticaInventoryPush, SystemState.SystemFault);
            }
        }
        
        public void LoadInventoryIntoShopify()
        {
            try
            {
                // Step 1 - Pull Shopify Inventory
                _acumaticaManager.PullInventory();

                // Step 2 - Load Acumatica Inventory into Shopify
                _inventorySyncManager.PushAcumaticaInventoryIntoShopify();

                _stateRepository.UpdateSystemState(
                    x => x.ShopifyInventoryPush, SystemState.Ok);
            }
            catch (Exception ex)
            {
                _logger.Error(ex);

                _stateRepository.UpdateSystemState(
                    x => x.ShopifyInventoryPush, SystemState.SystemFault);
            }
        }
        
        public void RunDiagnostics()
        {
            ConnectToShopify();
            PullAcumaticaReferenceData();
            SyncWarehouseAndLocation();
        }


        public void RealTimeSynchronization()
        {
            RunImpervious(() => _shopifyManager.PullOrdersAndCustomers());

            RunImpervious(() => _acumaticaManager.PullCustomersAndOrdersAndShipments());
            
            RunImpervious(() =>
            { 
                // Orders, Refunds, Payments
                _orderManager.RoutineOrdersSync();

                // Shipments / Fulfillments
                _orderManager.RoutineFulfillmentSync();
            });
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
    }
}

