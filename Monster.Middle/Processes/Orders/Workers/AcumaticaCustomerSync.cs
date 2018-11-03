using Monster.Middle.Persist.Multitenant;

namespace Monster.Middle.Processes.Orders.Workers
{
    public class AcumaticaCustomerSync
    {
        private readonly OrderRepository _orderRepository;

        public AcumaticaCustomerSync(OrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public void RunMatch()
        {
            var customers =
                _orderRepository.RetrieveShopifyCustomersUnsynced();

            foreach (var customer in customers)
            {
                var acumaticaCustomer =
                    _orderRepository.RetrieveAcumaticaCustomerByEmail(customer.ShopifyPrimaryEmail);

                if (acumaticaCustomer != null)
                {
                    acumaticaCustomer.UsrShopifyCustomer = customer;
                    _orderRepository.SaveChanges();
                }
            }
        }
    }
}
