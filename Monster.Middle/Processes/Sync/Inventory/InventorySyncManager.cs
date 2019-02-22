using System;
using System.Collections.Generic;
using Monster.Acumatica.Http;
using Monster.Middle.Processes.Sync.Inventory.Model;
using Monster.Middle.Processes.Sync.Inventory.Workers;
using Push.Foundation.Utilities.Logging;


namespace Monster.Middle.Processes.Sync.Inventory
{
    public class InventorySyncManager
    {
        private readonly AcumaticaHttpContext _acumaticaContext;
        private readonly WarehouseLocationSync _warehouseLocationSync;
        private readonly AcumaticaInventorySync _acumaticaInventorySync;        
        private readonly ShopifyInventorySync _shopifyInventorySync;
        private readonly IPushLogger _pushLogger;

        public InventorySyncManager(
                    AcumaticaHttpContext acumaticaContext,
                    AcumaticaInventorySync acumaticaInventorySync,
                    WarehouseLocationSync warehouseLocationSync,                
                    ShopifyInventorySync shopifyInventorySync, 
                    IPushLogger pushLogger)
        {
            _acumaticaContext = acumaticaContext;
            _acumaticaInventorySync = acumaticaInventorySync;
            _warehouseLocationSync = warehouseLocationSync;
            _shopifyInventorySync = shopifyInventorySync;
            _pushLogger = pushLogger;
        }
        
        
        public void SynchronizeWarehouseLocation()
        {
            _warehouseLocationSync.Run();
        }
        
        public void ImportIntoAcumatica(AcumaticaInventoryImportContext context)
        {
            AcumaticaSessionRun(() =>
            {
                _acumaticaInventorySync.Run(context);
            });
        }

        public void PushAcumaticaInventoryIntoShopify()
        {
            _shopifyInventorySync.Run();
        }

        public void AcumaticaSessionRun(Action action)
        {
            try
            {
                _acumaticaContext.Login();
                action();
            }
            catch (Exception ex)
            {
                _pushLogger.Error(ex);
                throw;
            }
            finally
            {
                if (_acumaticaContext.IsLoggedIn)
                {
                    _acumaticaContext.Logout();
                }
            }
        }
    }
}

