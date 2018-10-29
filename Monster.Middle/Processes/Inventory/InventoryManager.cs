﻿using Monster.Acumatica.Http;
using Monster.Middle.Processes.Inventory.Services;
using Monster.Middle.Processes.Inventory.Workers;
using Push.Foundation.Utilities.Logging;


namespace Monster.Middle.Processes.Inventory
{
    public class InventoryManager
    {
        private readonly AcumaticaHttpContext _acumaticaContext;
        private readonly AcumaticaWarehousePull _acumaticaWarehousePull;
        private readonly AcumaticaWarehouseSync _acumaticaWarehouseSync;
        private readonly AcumaticaInventoryPull _acumaticaInventoryPull;
        private readonly AcumaticaInventorySync _acumaticaInventorySync;
        
        private readonly ShopifyLocationPull _shopifyLocationPull;
        private readonly ShopifyLocationSync _shopifyLocationSync;
        private readonly ShopifyInventoryPull _shopifyInventoryPull;
        private readonly ShopifyInventorySync _shopifyInventorySync;

        private readonly InventoryStatusService _inventoryStatusService;

        private readonly IPushLogger _logger;

        public InventoryManager(
                AcumaticaHttpContext acumaticaContext,

                AcumaticaInventoryPull acumaticaInventoryPull,
                AcumaticaInventorySync acumaticaInventorySync,
                AcumaticaWarehousePull acumaticaWarehousePull,
                AcumaticaWarehouseSync acumaticaWarehouseSync,
                
                ShopifyInventoryPull shopifyInventoryPull,
                ShopifyInventorySync shopifyInventorySync,
                ShopifyLocationPull shopifyLocationPull,
                ShopifyLocationSync shopifyLocationSync,

                InventoryStatusService inventoryStatusService,

                IPushLogger logger)
        {
            _acumaticaContext = acumaticaContext;

            _acumaticaInventoryPull = acumaticaInventoryPull;
            _acumaticaInventorySync = acumaticaInventorySync;
            _acumaticaWarehousePull = acumaticaWarehousePull;
            _acumaticaWarehouseSync = acumaticaWarehouseSync;

            _shopifyInventoryPull = shopifyInventoryPull;
            _shopifyLocationPull = shopifyLocationPull;
            _shopifyLocationSync = shopifyLocationSync;
            _shopifyInventorySync = shopifyInventorySync;

            _inventoryStatusService = inventoryStatusService;

            _logger = logger;
        }


        public void InitialLoad()
        {
            _logger.Info("Inventory -> Baseline running...");

            // Warehouse and Location Pull
            _shopifyLocationPull.Run();
            _acumaticaContext.Begin();
            _acumaticaWarehousePull.Run();

            // Warehouse and Location Sync
            _acumaticaWarehouseSync.Run();
            _shopifyLocationSync.Run();

            // Status checkpoint
            var status = _inventoryStatusService.GetCurrentLocationStatus();
            if (!status.OK)
            {
                _logger.Info("Aborting Inventory -> Baseline:");
                _logger.Info(status.GetSynopsis());
                return;
            }

            // Products and Inventory Pull
            _shopifyInventoryPull.RunAll();
            _acumaticaInventoryPull.RunAll();
            
            // TODO - control this via a Preference
            LoadShopifyInventoryIntoAcumatica();
        }
        
        public void LoadShopifyInventoryIntoAcumatica()
        {
            // Synchronize Shopify Inventory to Acumatica
            _acumaticaInventorySync.Run();
            _acumaticaInventorySync.RunInventoryReceipts();

            // TODO - control this via a Preference
            _acumaticaInventorySync.RunInventoryReceiptsRelease();

            _acumaticaInventoryPull.RunUpdated();
        }

        public void UpdateLoad()
        {

        }
    }
}

