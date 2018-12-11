using System;
using Monster.Middle.Persist.Multitenant;
using Monster.Middle.Processes.Acumatica;
using Monster.Middle.Processes.Acumatica.Persist;
using Monster.Middle.Processes.Shopify;
using Monster.Middle.Processes.Shopify.Persist;
using Monster.Middle.Processes.Sync.Inventory;
using Monster.Middle.Processes.Sync.Inventory.Services;
using Monster.Middle.Processes.Sync.Orders;
using Push.Foundation.Utilities.Logging;

namespace Monster.Middle.Processes.Sync.Directors
{
    public class SyncDirector
    {
        private readonly ShopifyBatchRepository _shopifyBatchRepository;
        private readonly AcumaticaBatchRepository _acumaticaBatchRepository;
        private readonly AcumaticaManager _acumaticaManager;
        private readonly ShopifyManager _shopifyManager;
        private readonly InventoryManager _inventoryManager;
        private readonly OrderManager _orderManager;
        private readonly IPushLogger _logger;

        public SyncDirector(
                ShopifyBatchRepository shopifyBatchRepository, 
                AcumaticaBatchRepository acumaticaBatchRepository, 
                AcumaticaManager acumaticaManager, 
                ShopifyManager shopifyManager, 
                InventoryManager inventoryManager,
                OrderManager orderManager,
                InventoryStatusService inventoryStatusService, 
                JobRepository jobRepository,
                IPushLogger logger)
        {
            _shopifyBatchRepository = shopifyBatchRepository;
            _acumaticaBatchRepository = acumaticaBatchRepository;
            _acumaticaManager = acumaticaManager;
            _shopifyManager = shopifyManager;
            _inventoryManager = inventoryManager;
            _orderManager = orderManager;
            _logger = logger;
        }
        
        public void ResetBatchStates()
        {
            _shopifyBatchRepository.Reset();
            _acumaticaBatchRepository.Reset();
        }
        
        public void SyncWarehouseAndLocation()
        {
            // Step 1 - Pull Locations and Warehouses
            _acumaticaManager.PullWarehouses();
            _shopifyManager.PullLocations();

            // Step 2 - Synchronize Locations and Warehouses
            _inventoryManager.SynchronizeLocationOnly();
        }

        public void LoadInventoryIntoAcumatica()
        { 
            // Step 1 - Pull Shopify Inventory
            _shopifyManager.PullInventory();

            // Step 2 - Pull Acumatica Inventory
            _acumaticaManager.PullInventory();

            // Step 3 - Load Shopify Inventory into Acumatica as baseline
            _inventoryManager.PushShopifyInventoryIntoAcumatica();
        }
        
        public void LoadInventoryIntoShopify()
        {
            // Step 1 - Pull Shopify Inventory
            _acumaticaManager.PullInventory();

            // Step 2 - Load Acumatica Inventory into Shopify
            _inventoryManager.PushAcumaticaInventoryIntoShopify();
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

