using Monster.Acumatica.Http;
using Monster.Middle.Persist.Multitenant.Etc;
using Monster.Middle.Processes.Inventory.Workers;
using Monster.Middle.Processes.Orders.Workers;


namespace Monster.Middle.Processes.Orders
{
    public class OrderManager
    {
        private readonly BatchStateRepository _batchStateRepository;
        private readonly AcumaticaHttpContext _acumaticaContext;

        private readonly ShopifyCustomerPull _shopifyCustomerPull;
        private readonly ShopifyOrderPull _shopifyOrderPull;
        private readonly ShopifyFulfillmentSync _shopifyFulfillmentSync;

        private readonly AcumaticaCustomerPull _acumaticaCustomerPull;
        private readonly AcumaticaOrderPull _acumaticaOrderPull;
        private readonly AcumaticaCustomerSync _acumaticaCustomerSync;
        private readonly AcumaticaOrderSync _acumaticaOrderSync;
        private readonly AcumaticaShipmentPull _acumaticaShipmentPull;
        private readonly AcumaticaShipmentSync _acumaticaShipmentSync;
        private readonly AcumaticaInventorySync _acumaticaInventorySync;
        
        public OrderManager(
                BatchStateRepository batchStateRepository,
                AcumaticaHttpContext acumaticaContext,

                ShopifyCustomerPull shopifyCustomerPull, 
                ShopifyOrderPull shopifyOrderPull,
                ShopifyFulfillmentSync shopifyFulfillmentSync,

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
            _shopifyFulfillmentSync = shopifyFulfillmentSync;
            
            _acumaticaContext = acumaticaContext;

            _acumaticaCustomerPull = acumaticaCustomerPull;
            _acumaticaCustomerSync = acumaticaCustomerSync;
            _acumaticaInventorySync = acumaticaInventorySync;
            _acumaticaOrderPull = acumaticaOrderPull;
            _acumaticaOrderSync = acumaticaOrderSync;
            _acumaticaShipmentPull = acumaticaShipmentPull;
            _acumaticaShipmentSync = acumaticaShipmentSync;
        }


        public void Reset()
        {
            _batchStateRepository.ResetOrders();
        }


        public void SynchronizeInitial()
        {
            _shopifyCustomerPull.RunAutomatic();
            _shopifyOrderPull.RunAutomatic();

            _acumaticaContext.Login();

            // Pull down Acumatica Customers, Orders, Shipments, phew!
            _acumaticaCustomerPull.RunAutomatic();
            _acumaticaOrderPull.RunAutomatic();
            _acumaticaShipmentPull.RunAutomatic();

            // Automatically match Customer by email address (non-essential)
            _acumaticaCustomerSync.RunMatchByEmail();
            _acumaticaContext.Logout();
        }

        public void SynchronizeRoutine()
        {
            // Shopify Pull
            _shopifyCustomerPull.RunAutomatic();
            _shopifyOrderPull.RunAutomatic();
            
            // Acumatica Pull
            _acumaticaContext.Login();

            // Get the latest Acumatica Sales Orders for monitoring sake
            _acumaticaOrderPull.RunAutomatic();
            _acumaticaShipmentPull.RunAutomatic();
            
            // Acumatica Sync
            _acumaticaInventorySync.Run();
            _acumaticaOrderSync.Run();

            // TODO - this depends on whether the preference is to:
            // 1) Sync Fulfillments to Acumatica Shipments
            _acumaticaShipmentSync.RunShipments();
            _acumaticaShipmentSync.RunConfirmShipments();
            _acumaticaShipmentSync.RunSingleInvoicePerShipment();
            _acumaticaContext.Logout();

            // ...or to:
            // 2) Sync Shipments to Shopify Fulfillments
            //_shopifyFulfillmentSync.Run();
        }

        public void SingleOrderPush(long shopifyOrderId)
        {
            _shopifyOrderPull.Run(shopifyOrderId);

            _acumaticaContext.Login();
            _acumaticaOrderSync.RunByShopifyId(shopifyOrderId);
        }        
    }
}

