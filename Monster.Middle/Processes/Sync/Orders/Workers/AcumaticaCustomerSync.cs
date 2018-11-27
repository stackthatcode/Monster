using Monster.Middle.Persist.Multitenant.Sync;
using Monster.Middle.Processes.Acumatica.Persist;
using Monster.Middle.Processes.Shopify.Persist;

namespace Monster.Middle.Processes.Sync.Orders.Workers
{
    public class AcumaticaCustomerSync
    {
        private readonly AcumaticaOrderRepository _acumaticaOrderRepository;
        private readonly ShopifyOrderRepository _shopifyOrderRepository;
        private readonly SyncOrderRepository _syncOrderRepository;

        public AcumaticaCustomerSync(
                AcumaticaOrderRepository acumaticaOrderRepository, 
                ShopifyOrderRepository shopifyOrderRepository, 
                SyncOrderRepository syncOrderRepository)
        {
            _acumaticaOrderRepository = acumaticaOrderRepository;
            _shopifyOrderRepository = shopifyOrderRepository;
            _syncOrderRepository = syncOrderRepository;
        }

        public void RunMatchByEmail()
        {
            var shopifyCustomers = _syncOrderRepository.RetrieveCustomersUnsynced();

            foreach (var shopifyCustomer in shopifyCustomers)
            {
                var acumaticaCustomer =
                    _acumaticaOrderRepository
                        .RetrieveCustomerByEmail(shopifyCustomer.ShopifyPrimaryEmail);

                if (acumaticaCustomer != null)
                {
                    _syncOrderRepository
                        .InsertCustomerSync(shopifyCustomer, acumaticaCustomer);
                }
            }
        }
    }
}
