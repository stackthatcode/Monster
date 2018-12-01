using System;
using Monster.Middle.Persist.Multitenant;
using Monster.Middle.Persist.Multitenant.Model;
using Monster.Middle.Processes.Acumatica;
using Monster.Middle.Processes.Acumatica.Persist;
using Monster.Middle.Processes.Shopify;
using Monster.Middle.Processes.Shopify.Persist;
using Monster.Middle.Processes.Sync.Inventory;
using Monster.Middle.Processes.Sync.Inventory.Services;
using Monster.Middle.Processes.Sync.Orders;
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
        private readonly OrderManager _orderManager;
        private readonly TenantContext _tenantContext;
        private readonly TenantRepository _tenantRepository;
        private readonly InventoryStatusService _inventoryStatusService;
        private readonly JobRepository _jobRepository;
        private readonly IPushLogger _logger;

        private static NamedLock 
            _warehouseLocationLock = new NamedLock("");


        public SyncDirector(
                ShopifyBatchRepository shopifyBatchRepository, 
                AcumaticaBatchRepository acumaticaBatchRepository, 
                AcumaticaManager acumaticaManager, 
                ShopifyManager shopifyManager, 
                InventoryManager inventoryManager,
                OrderManager orderManager,
                TenantContext tenantContext,
                TenantRepository tenantRepository,
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
            _tenantContext = tenantContext;
            _tenantRepository = tenantRepository;
            _logger = logger;
            _inventoryStatusService = inventoryStatusService;
            _jobRepository = jobRepository;
        }


        // Run-once jobs
        //
        public void ResetBatchStates(Guid tenantId)
        {
            _jobRepository.Clear();
            _tenantContext.Initialize(tenantId);
            _shopifyBatchRepository.Reset();
            _acumaticaBatchRepository.Reset();
        }
        
        public void SyncWarehouseAndLocation(Guid tenantId, bool updateStatus = true)
        {
            ExecuteRunOnceJob(
                QueuedJobType.SyncWarehouseAndLocation,
                () => {
                    _tenantContext.Initialize(tenantId);

                    // Step 1 - Pull Locations and Warehouses
                    _acumaticaManager.PullWarehouses();
                    _shopifyManager.PullLocations();

                    // Step 2 - Synchronize Locations and Warehouses
                    _inventoryManager.SynchronizeLocationOnly();
                },
                updateStatus);
        }

        public void LoadInventoryIntoAcumatica(Guid tenantId, bool updateStatus = true)
        {
            ExecuteRunOnceJob(
                QueuedJobType.LoadInventoryIntoAcumatica,
                () => {

                    _tenantContext.Initialize(tenantId);

                    // Step 1 - Pull Shopify Inventory
                    _shopifyManager.PullInventory();

                    // Step 2 - Pull Acumatica Inventory
                    _acumaticaManager.PullInventory();

                    // Step 3 - Load Shopify Inventory into Acumatica as baseline
                    _inventoryManager.PushShopifyInventoryIntoAcumatica();
                    },
                    updateStatus);
        }
        
        public void LoadInventoryIntoShopify(Guid tenantId, bool updateStatus = true)
        {
            ExecuteRunOnceJob(
                QueuedJobType.LoadInventoryIntoShopify,
                () => {
                    _tenantContext.Initialize(tenantId);

                    // Step 1 - Pull Shopify Inventory
                    _acumaticaManager.PullInventory();

                    // Step 2 - Load Acumatica Inventory into Shopify
                    _inventoryManager.PushAcumaticaInventoryIntoShopify();
                },
                updateStatus);
        }
        
        private void ExecuteRunOnceJob(
                    int queueJobTypeId, Action task, bool updateStatus)
        {
            try
            {
                task();

                if (updateStatus)
                {
                    _jobRepository.UpdateStatus(queueJobTypeId, JobStatus.Complete);
                }
            }
            catch (Exception ex)
            {
                if (updateStatus)
                {
                    _jobRepository.UpdateStatus(queueJobTypeId, JobStatus.Failed);
                }

                _logger.Error(ex);
                throw;
            }
        }



        public static readonly 
                NamedLock _routineSyncLock = new NamedLock("RoutineSynchHarness");
        
        // TODO - add NameLocking based on Tenant Id
        public void RoutineSynchHarness(Guid tenantId)
        {
            try
            {
                if (!_routineSyncLock.Acquire(tenantId.ToString()))
                {
                    _logger.Info($"Failed to acquired RoutineSynchHarness for {tenantId}");
                    return;
                }

                RoutineSync(tenantId);
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                throw;
            }
            finally
            {
                _routineSyncLock.Free(tenantId.ToString());
            }            
        }

        public void RoutineSync(Guid tenantId)
        {
            _tenantContext.Initialize(tenantId);

            Execute(() => _shopifyManager.PullOrdersAndCustomers());

            Execute(() => _acumaticaManager.PullCustomerAndOrdersAndShipments());

            Execute(() =>
            {
                // Step 1 - Load Acumatica Inventory into Shopify
                _inventoryManager.PushAcumaticaInventoryIntoShopify();

                // Step 2 (optional) - Load Products into Acumatica
                //_orderManager.LoadShopifyProductsIntoAcumatica();

                // Step 3 - Load Orders, Refunds, Payments and Shipments
                _orderManager.RoutineOrdersSync();
            });
        }

        private void Execute(Action task)
        {
            try
            {
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

