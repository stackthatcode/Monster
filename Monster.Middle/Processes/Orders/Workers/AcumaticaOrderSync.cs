using System.Linq;
using Monster.Acumatica.Api;
using Monster.Acumatica.Api.Common;
using Monster.Acumatica.Api.Customer;
using Monster.Acumatica.Api.SalesOrder;
using Monster.Middle.Persist.Multitenant;
using Push.Foundation.Utilities.Json;

namespace Monster.Middle.Processes.Orders.Workers
{
    public class AcumaticaOrderSync
    {
        private readonly OrderRepository _orderRepository;
        private readonly CustomerClient _customerClient;
        private readonly SalesOrderClient _salesOrderClient;

        public AcumaticaOrderSync(OrderRepository orderRepository, CustomerClient customerClient, SalesOrderClient salesOrderClient)
        {
            _orderRepository = orderRepository;
            _customerClient = customerClient;
            _salesOrderClient = salesOrderClient;
        }

        public void Run()
        {
            var shopifyOrders = 
                _orderRepository.RetrieveShopifyOrdersNotSync();

            foreach (var order in shopifyOrders)
            {
                var customer = order.UsrShopifyCustomer;
                UsrAcumaticaCustomer acumaticaCustomer;

                if (!customer.UsrAcumaticaCustomers.Any())
                {
                    acumaticaCustomer = PushCustomer(customer);
                }
                else
                {
                    acumaticaCustomer 
                        = customer.UsrAcumaticaCustomers.FirstOrDefault();
                }

                // TODO - write actual Order
            }
        }


        public UsrAcumaticaCustomer 
                PushCustomer(UsrShopifyCustomer shopifyCustomerRecord)
        {
            var shopifyCustomer =
                shopifyCustomerRecord
                    .ShopifyJson
                    .DeserializeFromJson<Push.Shopify.Api.Customer.Customer>();
            
            var name = shopifyCustomer.first_name + " " + shopifyCustomer.last_name;
            var shopifyAddress = shopifyCustomer.default_address;

            var customer = new Customer();
            customer.CustomerName = name.ToValue();
            
            var address = new Address();
            address.AddressLine1 = shopifyAddress.address1.ToValue();
            address.AddressLine2 = shopifyAddress.address2.ToValue();
            address.City = shopifyAddress.city.ToValue();
            address.State = shopifyAddress.province.ToValue();
            address.PostalCode = shopifyAddress.zip.ToValue();

            var mainContact = new Contact();
            mainContact.Address = address;
            mainContact.FirstName = shopifyCustomer.first_name.ToValue();
            mainContact.LastName = shopifyCustomer.last_name.ToValue();
            mainContact.Phone1 = shopifyCustomer.phone.ToValue();
            mainContact.Email = shopifyCustomer.email.ToValue();

            customer.MainContact = mainContact;

            var resultJson 
                = _customerClient.AddNewCustomer(customer.SerializeToJson());

            var newAcumaticaCustomer = resultJson.DeserializeFromJson<Customer>();
            var acumaticaMonsterRecord = newAcumaticaCustomer.ToMonsterRecord();
            acumaticaMonsterRecord.UsrShopifyCustomer = shopifyCustomerRecord;

            _orderRepository.InsertAcumaticaCustomer(acumaticaMonsterRecord);
            return acumaticaMonsterRecord;
        }
    }
}

