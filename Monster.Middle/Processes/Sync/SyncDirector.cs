using System;
using Monster.Middle.Hangfire;
using Monster.Middle.Persist.Multitenant;
using Monster.Middle.Processes.Acumatica;
using Monster.Middle.Processes.Acumatica.Persist;
using Monster.Middle.Processes.Shopify;
using Monster.Middle.Processes.Shopify.Persist;
using Monster.Middle.Processes.Sync.Inventory;
using Monster.Middle.Processes.Sync.Inventory.Services;
using Monster.Middle.Processes.Sync.Orders;
using Push.Foundation.Utilities.Logging;

namespace Monster.Middle.Processes.Sync
{
    public class SyncDirector
    {
        private readonly StateRepository _stateRepository;
        private readonly ShopifyBatchRepository _shopifyBatchRepository;
        private readonly AcumaticaBatchRepository _acumaticaBatchRepository;
        private readonly AcumaticaManager _acumaticaManager;
        private readonly ShopifyManager _shopifyManager;
        private readonly InventoryManager _inventoryManager;
        private readonly InventoryStatusService _inventoryStatusService;
        private readonly OrderManager _orderManager;
        private readonly IPushLogger _logger;

        public SyncDirector(
                StateRepository stateRepository,
                ShopifyBatchRepository shopifyBatchRepository, 
                AcumaticaBatchRepository acumaticaBatchRepository, 
                AcumaticaManager acumaticaManager, 
                ShopifyManager shopifyManager, 
                InventoryStatusService inventoryStatusService,
                InventoryManager inventoryManager,
                OrderManager orderManager,
                IPushLogger logger)
        {
            _stateRepository = stateRepository;
            _shopifyBatchRepository = shopifyBatchRepository;
            _acumaticaBatchRepository = acumaticaBatchRepository;
            _acumaticaManager = acumaticaManager;
            _shopifyManager = shopifyManager;
            _inventoryManager = inventoryManager;
            _inventoryStatusService = inventoryStatusService;
            _orderManager = orderManager;
            _logger = logger;
        }
        
        public void ResetBatchStates()
        {
            _shopifyBatchRepository.Reset();
            _acumaticaBatchRepository.Reset();
        }

        public void PullAcumaticaSettings()
        {
            try
            {
                _acumaticaManager.PullSettings();
                _stateRepository
                    .UpdateSystemState(x => x.WarehouseSync, SystemState.Ok);
            }
            catch (Exception ex)
            {
                _logger.Error(ex);

                _stateRepository.UpdateSystemState(
                    x => x.WarehouseSync, SystemState.SystemFault);
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
                _inventoryManager.SynchronizeLocationOnly();

                // Step 3 - Determine resultant System State
                var status = _inventoryStatusService.CurrentWarehouseSyncStatus();
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
                _inventoryManager.PushInventoryIntoAcumatica();

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
                _inventoryManager.PushAcumaticaInventoryIntoShopify();

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
        
        public void RoutineSync()
        {
            RunImpervious(() => _shopifyManager.PullOrdersAndCustomers());

            RunImpervious(() => _acumaticaManager.PullCustomerAndOrdersAndShipments());

            RunImpervious(() =>
            {
                // Step 1 - Load Acumatica Inventory into Shopify
                _inventoryManager.PushAcumaticaInventoryIntoShopify();

                // Step 2 (optional) - Load Products into Acumatica
                //_orderManager.LoadShopifyProductsIntoAcumatica();

                // Step 3 - Load Orders, Refunds, Payments and Shipments
                _orderManager.RoutineOrdersSync();
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

