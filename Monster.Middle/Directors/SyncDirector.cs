using System;
using Monster.Middle.Persist.Multitenant;
using Monster.Middle.Persist.Multitenant.Model;
using Monster.Middle.Processes.Acumatica;
using Monster.Middle.Processes.Acumatica.Persist;
using Monster.Middle.Processes.Shopify;
using Monster.Middle.Processes.Shopify.Persist;
using Monster.Middle.Processes.Sync.Inventory;
using Monster.Middle.Processes.Sync.Inventory.Services;
using Monster.Middle.Services;
using Push.Foundation.Utilities.Logging;

namespace Monster.Middle.Directors
{
    public class SyncDirector
    {
        private readonly ShopifyBatchRepository _shopifyBatchRepository;
        private readonly AcumaticaBatchRepository _acumaticaBatchRepository;
        private readonly AcumaticaManager _acumaticaManager;
        private readonly ShopifyManager _shopifyManager;
        private readonly InventoryManager _inventoryManager;
        private readonly TenantContext _tenantContext;
        private readonly TenantRepository _tenantRepository;
        private readonly InventoryStatusService _inventoryStatusService;
        private readonly IPushLogger _logger;


        public SyncDirector(
                ShopifyBatchRepository shopifyBatchRepository, 
                AcumaticaBatchRepository acumaticaBatchRepository, 
                AcumaticaManager acumaticaManager, 
                ShopifyManager shopifyManager, 
                InventoryManager inventoryManager, 
                TenantContext tenantContext,
                TenantRepository tenantRepository,
                IPushLogger logger, 
                InventoryStatusService inventoryStatusService)
        {
            _shopifyBatchRepository = shopifyBatchRepository;
            _acumaticaBatchRepository = acumaticaBatchRepository;
            _acumaticaManager = acumaticaManager;
            _shopifyManager = shopifyManager;
            _inventoryManager = inventoryManager;
            _tenantContext = tenantContext;
            _tenantRepository = tenantRepository;
            _logger = logger;
            _inventoryStatusService = inventoryStatusService;
        }


        public void ResetBatchStates(Guid tenantId)
        {
            Execute(tenantId, () =>
            {
                _shopifyBatchRepository.Reset();
                _acumaticaBatchRepository.Reset();
            });
        }
        
        public void SyncWarehouseAndLocation(Guid tenantId)
        {
            try
            {
                _tenantContext.Initialize(tenantId);

                // Step 1 - Pull Locations and Warehouses
                _acumaticaManager.PullWarehouses();
                _shopifyManager.PullLocations();

                // Step 2 - Synchronize Locations and Warehouses
                _inventoryManager.SynchronizeLocationOnly();

                // Update the Job Status
                _tenantRepository.UpdateWarehouseSyncStatus(JobStatus.Complete);
            }
            catch (Exception ex)
            {
                _tenantRepository.UpdateWarehouseSyncStatus(JobStatus.Failed);
                _logger.Error(ex);
                throw;
            }
        }

        public void LoadInventoryInAcumatica(Guid tenantId)
        {
            Execute(tenantId, () =>
            {
                // Step 1 - Pull Shopify Inventory
                _shopifyManager.PullInventory();

                // Step 2 - Pull Acumatica Inventory
                _acumaticaManager.PullInventory();

                // Step 3 - Load Shopify Inventory into Acumatica as baseline
                _inventoryManager.PushShopifyInventoryIntoAcumatica();                
            });
        }

        public void SyncInventoryWithShopify(Guid tenantId)
        {
            Execute(tenantId, () =>
            {
                // Step 1 - Pull Shopify Inventory
                _acumaticaManager.PullInventory();

                // Step 2 - Load Acumatica Inventory into Shopify
                _inventoryManager.PushAcumaticaInventoryIntoShopify();
            });
        }
        
        private void Execute(Guid tenantId, Action task)            
        {
            try
            {
                _tenantContext.Initialize(tenantId);
                task();
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                throw;
            }
        }
    }
}
