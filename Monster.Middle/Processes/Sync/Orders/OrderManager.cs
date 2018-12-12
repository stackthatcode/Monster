using Monster.Acumatica.Http;
using Monster.Middle.Persist.Multitenant;
using Monster.Middle.Processes.Sync.Inventory.Workers;
using Monster.Middle.Processes.Sync.Orders.Workers;

namespace Monster.Middle.Processes.Sync.Orders
{
    public class OrderManager
    {
        private readonly TenantRepository _tenantRepository;
        private readonly AcumaticaHttpContext _acumaticaContext;

        private readonly ShopifyFulfillmentSync _shopifyFulfillmentSync;

        private readonly AcumaticaCustomerSync _acumaticaCustomerSync;
        private readonly AcumaticaOrderSync _acumaticaOrderSync;
        private readonly AcumaticaShipmentSync _acumaticaShipmentSync;
        private readonly AcumaticaInventorySync _acumaticaInventorySync;
        private readonly AcumaticaPaymentSync _acumaticaPaymentSync;
        private readonly AcumaticaRefundSync _acumaticaRefundSync;

        public OrderManager(
                TenantRepository tenantRepository,

                AcumaticaHttpContext acumaticaContext,
                AcumaticaCustomerSync acumaticaCustomerSync,
                AcumaticaOrderSync acumaticaOrderSync,
                AcumaticaShipmentSync acumaticaShipmentSync,
                AcumaticaInventorySync acumaticaInventorySync,
                AcumaticaRefundSync acumaticaRefundSync, 
                AcumaticaPaymentSync acumaticaPaymentSync,

                ShopifyFulfillmentSync shopifyFulfillmentSync)
        {
            _tenantRepository = tenantRepository;
            _acumaticaContext = acumaticaContext;
            _acumaticaCustomerSync = acumaticaCustomerSync;
            _acumaticaInventorySync = acumaticaInventorySync;
            _acumaticaRefundSync = acumaticaRefundSync;
            _acumaticaPaymentSync = acumaticaPaymentSync;
            _acumaticaOrderSync = acumaticaOrderSync;
            _acumaticaShipmentSync = acumaticaShipmentSync;

            _shopifyFulfillmentSync = shopifyFulfillmentSync;
        }

        
        public void InitialSync()
        {
            // Automatically match Customer by email address (non-essential)
            _acumaticaCustomerSync.RunMatchByEmail();
        }

        public void LoadShopifyProductsIntoAcumatica()
        {
            _acumaticaContext.Login();

            _acumaticaInventorySync.Run();

            _acumaticaContext.Logout();
        }

        public void RoutineOrdersSync()
        {
            var preferences = _tenantRepository.RetrievePreferences();
            var fulfilledInAcumatica = preferences.FulfillmentInAcumatica.Value;

            _acumaticaContext.Login();

            // Load Sales Orders into Acumatica
            _acumaticaOrderSync.Run();

            if (!fulfilledInAcumatica)
            { 
                // Sync Fulfillments to Acumatica Shipments
                _acumaticaShipmentSync.RunShipments();
                _acumaticaShipmentSync.RunConfirmShipments();
                _acumaticaShipmentSync.RunSingleInvoicePerShipmentSalesRef();
            }

            // Synchronize Payments and Refunds
            _acumaticaPaymentSync.Run();
            _acumaticaRefundSync.Run();

            _acumaticaContext.Logout();


            // Sync Shipments to Shopify Fulfillments
            if (fulfilledInAcumatica)
            {
                _shopifyFulfillmentSync.Run();
            }
        }

        public void SingleOrderPush(long shopifyOrderId)
        {
            _acumaticaContext.Login();
            _acumaticaOrderSync.RunByShopifyId(shopifyOrderId);
            _acumaticaContext.Logout();
        }
    }
}

