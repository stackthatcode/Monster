using Monster.Middle.Persist.Multitenant;
using Monster.Middle.Persist.Multitenant.Acumatica;
using Monster.Middle.Persist.Multitenant.Shopify;

namespace Monster.Middle.Processes.Orders.Workers
{
    public class AcumaticaCustomerSync
    {
        private readonly AcumaticaOrderRepository _acumaticaOrderRepository;
        private readonly ShopifyOrderRepository _shopifyOrderRepository;

        public AcumaticaCustomerSync(
                AcumaticaOrderRepository acumaticaOrderRepository, 
                ShopifyOrderRepository shopifyOrderRepository)
        {
            _acumaticaOrderRepository = acumaticaOrderRepository;
            _shopifyOrderRepository = shopifyOrderRepository;
        }

        public void RunMatch()
        {
            var customers =
                _shopifyOrderRepository.RetrieveCustomersUnsynced();

            foreach (var customer in customers)
            {
                var acumaticaCustomer =
                    _acumaticaOrderRepository
                        .RetrieveCustomerByEmail(customer.ShopifyPrimaryEmail);

                if (acumaticaCustomer != null)
                {
                    acumaticaCustomer.UsrShopifyCustomer = customer;
                    _acumaticaOrderRepository.SaveChanges();
                }
            }
        }
    }
}
