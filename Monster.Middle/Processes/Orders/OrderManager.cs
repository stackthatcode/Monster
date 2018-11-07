using Monster.Acumatica.Http;
using Monster.Middle.Persist.Multitenant;
using Monster.Middle.Processes.Inventory.Workers;
using Monster.Middle.Processes.Orders.Workers;

namespace Monster.Middle.Processes.Orders
{
    public class OrderManager
    {
        private readonly BatchStateRepository _batchStateRepository;
        private readonly ShopifyCustomerPull _shopifyCustomerPull;
        private readonly ShopifyOrderPull _shopifyOrderPull;
        private readonly AcumaticaHttpContext _acumaticaContext;
        private readonly AcumaticaCustomerPull _acumaticaCustomerPull;
        private readonly AcumaticaOrderPull _acumaticaOrderPull;
        private readonly AcumaticaCustomerSync _acumaticaCustomerSync;
        private readonly AcumaticaOrderSync _acumaticaOrderSync;
        private readonly AcumaticaShipmentPull _acumaticaShipmentPull;
        private readonly AcumaticaShipmentSync _acumaticaShipmentSync;
        private readonly AcumaticaInventorySync _acumaticaInventorySync;

        public OrderManager(
                BatchStateRepository batchStateRepository,
                ShopifyCustomerPull shopifyCustomerPull, 
                ShopifyOrderPull shopifyOrderPull,

                AcumaticaHttpContext acumaticaContext,

                AcumaticaCustomerPull acumaticaCustomerPull,
                AcumaticaCustomerSync acumaticaCustomerSync,
                AcumaticaOrderPull acumaticaOrderPull,
                AcumaticaOrderSync acumaticaOrderSync,
                AcumaticaShipmentPull acumaticaShipmentPull,
                AcumaticaShipmentSync acumaticaShipmentSync,
                AcumaticaInventorySync acumaticaInventorySync)
        {
            _batchStateRepository = batchStateRepository;

            _shopifyCustomerPull = shopifyCustomerPull;
            _shopifyOrderPull = shopifyOrderPull;

            _acumaticaContext = acumaticaContext;

            _acumaticaCustomerPull = acumaticaCustomerPull;
            _acumaticaCustomerSync = acumaticaCustomerSync;
            _acumaticaInventorySync = acumaticaInventorySync;
            _acumaticaOrderPull = acumaticaOrderPull;
            _acumaticaOrderSync = acumaticaOrderSync;
            _acumaticaShipmentPull = acumaticaShipmentPull;
            _acumaticaShipmentSync = acumaticaShipmentSync;
        }


        public void Baseline()
        {
            _batchStateRepository.ResetOrderBatchState();

            //_shopifyCustomerPull.RunAll();
            //_shopifyOrderPull.RunAll();

            _acumaticaContext.Begin();

            // Any Products detected and loaded from Shopify Orders are synced            
            _acumaticaCustomerPull.RunAll();
            _acumaticaOrderPull.RunAll();
            _acumaticaShipmentPull.RunAll();
            
            // Optional...
            //_acumaticaInventorySync.Run();
            //_acumaticaCustomerSync.RunMatch();
        }

        public void Incremental()
        {
            _shopifyCustomerPull.RunUpdated();
            _shopifyOrderPull.RunUpdated();
            
            _acumaticaContext.Begin();
            
            // Any Products detected and loaded from Shopify Orders are synced
            _acumaticaInventorySync.Run();

            _acumaticaOrderPull.RunUpdated();
            _acumaticaOrderSync.Run();
        }

        public void SingleOrderPush(long shopifyOrderId)
        {
            _shopifyOrderPull.Run(shopifyOrderId);

            _acumaticaContext.Begin();
            _acumaticaOrderSync.RunByShopifyId(shopifyOrderId);
        }
        
    }
}

