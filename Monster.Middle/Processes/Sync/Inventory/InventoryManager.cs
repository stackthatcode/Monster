using Monster.Acumatica.Http;
using Monster.Middle.Processes.Sync.Inventory.Services;
using Monster.Middle.Processes.Sync.Inventory.Workers;
using Push.Foundation.Utilities.Logging;


namespace Monster.Middle.Processes.Sync.Inventory
{
    public class InventoryManager
    {
        private readonly AcumaticaHttpContext _acumaticaContext;
        private readonly AcumaticaWarehouseSync _acumaticaWarehouseSync;
        private readonly AcumaticaInventorySync _acumaticaInventorySync;
        
        private readonly ShopifyLocationSync _shopifyLocationSync;
        private readonly ShopifyInventorySync _shopifyInventorySync;

        private readonly StatusService _inventoryStatusService;
        private readonly IPushLogger _logger;


        public InventoryManager(
                AcumaticaHttpContext acumaticaContext,
                AcumaticaInventorySync acumaticaInventorySync,
                AcumaticaWarehouseSync acumaticaWarehouseSync,
                
                ShopifyLocationSync shopifyLocationSync,
                ShopifyInventorySync shopifyInventorySync,

                StatusService inventoryStatusService,
                IPushLogger logger)
        {
            _acumaticaContext = acumaticaContext;
            _acumaticaInventorySync = acumaticaInventorySync;
            _acumaticaWarehouseSync = acumaticaWarehouseSync;

            _shopifyLocationSync = shopifyLocationSync;
            _shopifyInventorySync = shopifyInventorySync;

            _inventoryStatusService = inventoryStatusService;
            _logger = logger;
        }
        
        
        public void SynchronizeLocationOnly()
        {
            _acumaticaWarehouseSync.Run();
            _shopifyLocationSync.Run();
        }

        public bool LocationStatusCheck()
        {
            // Status checkpoint
            var status = _inventoryStatusService.WarehouseSyncStatus();
            if (!status.IsOk)
            {
                _logger.Info(status.GetSynopsis());
                return false;
            }
            else
            {
                return true;
            }
        }
        
        public void PushInventoryIntoAcumatica()
        {
            _acumaticaContext.Login();

            // Synchronize Shopify Inventory to Acumatica
            _acumaticaInventorySync.Run();

            // TODO - control this via a Preference
            _acumaticaInventorySync.RunInventoryReceipts();

            // TODO - control this via a Preference
            _acumaticaInventorySync.RunInventoryReceiptsRelease();

            _acumaticaContext.Logout();
        }

        public void PushAcumaticaInventoryIntoShopify()
        {
            // Finally, push Acumatica Inventory into Shopify
            _shopifyInventorySync.Run();

        }
    }
}

