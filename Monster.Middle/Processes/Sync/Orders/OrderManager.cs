using System;
using Monster.Acumatica.Http;
using Monster.Middle.Persist.Multitenant;
using Monster.Middle.Processes.Sync.Inventory.Workers;
using Monster.Middle.Processes.Sync.Orders.Workers;
using Push.Foundation.Utilities.Logging;

namespace Monster.Middle.Processes.Sync.Orders
{
    public class OrderManager
    {
        private readonly PreferencesRepository _preferencesRepository;
        private readonly ShopifyFulfillmentSync _shopifyFulfillmentSync;

        private readonly AcumaticaHttpContext _acumaticaContext;
        private readonly AcumaticaCustomerSync _acumaticaCustomerSync;
        private readonly AcumaticaOrderSync _acumaticaOrderSync;
        private readonly AcumaticaShipmentSync _acumaticaShipmentSync;
        private readonly AcumaticaInventorySync _acumaticaInventorySync;
        private readonly AcumaticaOrderPaymentSync _acumaticaPaymentSync;
        private readonly AcumaticaRefundSync _acumaticaRefundSync;

        private readonly IPushLogger _logger;


        public OrderManager(
                AcumaticaHttpContext acumaticaContext,
                AcumaticaCustomerSync acumaticaCustomerSync,
                AcumaticaOrderSync acumaticaOrderSync,
                AcumaticaShipmentSync acumaticaShipmentSync,
                AcumaticaInventorySync acumaticaInventorySync,
                AcumaticaRefundSync acumaticaRefundSync, 
                AcumaticaOrderPaymentSync acumaticaPaymentSync,

                ShopifyFulfillmentSync shopifyFulfillmentSync, 
                PreferencesRepository preferencesRepository,
                
                IPushLogger logger)
        {
            _acumaticaContext = acumaticaContext;
            _acumaticaCustomerSync = acumaticaCustomerSync;
            _acumaticaInventorySync = acumaticaInventorySync;
            _acumaticaRefundSync = acumaticaRefundSync;
            _acumaticaPaymentSync = acumaticaPaymentSync;
            _acumaticaOrderSync = acumaticaOrderSync;
            _acumaticaShipmentSync = acumaticaShipmentSync;

            _shopifyFulfillmentSync = shopifyFulfillmentSync;
            _preferencesRepository = preferencesRepository;
            _logger = logger;
        }

        
        public void InitialSync()
        {
            // Automatically match Customer by email address (non-essential)
            _acumaticaCustomerSync.Run();
        }

        public void LoadShopifyProductsIntoAcumatica()
        {
            _acumaticaContext.Login();

            _acumaticaInventorySync.Run();

            _acumaticaContext.Logout();
        }

        public void RoutineOrdersSync()
        {
            AcumaticaSessionRun(() =>
            {
                _acumaticaCustomerSync.Run();
                _acumaticaOrderSync.Run();

                _acumaticaPaymentSync.RunPaymentsForOrders();

                _acumaticaRefundSync.RunReturns();
                _acumaticaRefundSync.RunCancellations();
            });
        }

        public void RoutineFulfillmentSync()
        {
            var preferences = _preferencesRepository.RetrievePreferences();
            var fulfilledInAcumatica = preferences.FulfillmentInAcumatica.Value;

            // Sync Fulfillments to Acumatica Shipments
            if (!fulfilledInAcumatica)
            {
                AcumaticaSessionRun(() =>
                {
                    _acumaticaShipmentSync.RunShipments();
                    _acumaticaShipmentSync.RunConfirmShipments();
                    _acumaticaShipmentSync.RunSingleInvoicePerShipmentSalesRef();
                });
            }

            // Sync Shipments to Shopify Fulfillments
            if (fulfilledInAcumatica)
            {
                _shopifyFulfillmentSync.Run();
            }
        }

        public void SingleOrderPush(long shopifyOrderId)
        {
            AcumaticaSessionRun(() =>
            {
                _acumaticaOrderSync.RunByShopifyId(shopifyOrderId);
            });
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
                _logger.Error(ex);
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

