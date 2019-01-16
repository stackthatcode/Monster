using System;
using System.Linq;
using Monster.Acumatica.Api;
using Monster.Acumatica.Api.Common;
using Monster.Acumatica.Api.Customer;
using Monster.Acumatica.Api.SalesOrder;
using Monster.Middle.Persist.Multitenant;
using Monster.Middle.Processes.Acumatica.Persist;
using Monster.Middle.Processes.Acumatica.Workers;
using Monster.Middle.Processes.Shopify.Persist;
using Monster.Middle.Processes.Sync.Extensions;
using Push.Foundation.Utilities.Json;

namespace Monster.Middle.Processes.Sync.Orders.Workers
{
    public class AcumaticaCustomerSync
    {
        private readonly AcumaticaCustomerPull _acumaticaCustomerPull;
        private readonly AcumaticaOrderRepository _acumaticaOrderRepository;
        private readonly SyncOrderRepository _syncOrderRepository;
        private readonly CustomerClient _customerClient;

        public AcumaticaCustomerSync(
                AcumaticaCustomerPull acumaticaCustomerPull,
                AcumaticaOrderRepository acumaticaOrderRepository, 
                ShopifyOrderRepository shopifyOrderRepository, 
                SyncOrderRepository syncOrderRepository, 
                CustomerClient customerClient)
        {
            _acumaticaCustomerPull = acumaticaCustomerPull;
            _acumaticaOrderRepository = acumaticaOrderRepository;
            _syncOrderRepository = syncOrderRepository;
            _customerClient = customerClient;
        }

        public void Run()
        {
            var notLoadedCustomers 
                = _syncOrderRepository.RetrieveCustomersWithOrdersNotLoaded();

            foreach (var shopifyCustomer in notLoadedCustomers)
            {
                PushCustomer(shopifyCustomer);
            }

            var customersNeedingUpdate 
                = _syncOrderRepository.RetrieveCustomersNeedingUpdate();

            foreach (var shopifyCustomer in customersNeedingUpdate)
            {
                PushCustomer(shopifyCustomer);
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
            customer.AccountRef = 
                $"Shopify Customer #{shopifyCustomer.id}".ToValue();

            var acumaticaCustomerRecord = shopifyCustomerRecord.Match();
            if (acumaticaCustomerRecord != null)
            {
                customer.CustomerID 
                    = acumaticaCustomerRecord.AcumaticaCustomerId.ToValue();
            }

            // Push Customer to Acumatica API
            var customerResultJson
                = _customerClient.WriteCustomer(customer.SerializeToJson());
            var customerResult = customerResultJson.DeserializeFromJson<Customer>();
            
            // Create SQL footprint
            using (var transaction = _syncOrderRepository.BeginTransaction())
            {
                var output = 
                    _acumaticaCustomerPull.UpsertCustomerToPersist(customerResult);

                var existingSync = output.UsrShopAcuCustomerSyncs.FirstOrDefault();

                if (existingSync == null)
                {
                    var syncRecord = new UsrShopAcuCustomerSync();
                    syncRecord.UsrShopifyCustomer = shopifyCustomerRecord;
                    syncRecord.UsrAcumaticaCustomer = output;
                    syncRecord.DateCreated = DateTime.UtcNow;
                    syncRecord.LastUpdated = DateTime.UtcNow;
                    _syncOrderRepository.InsertCustomerSync(syncRecord);
                }
                else
                {
                    existingSync.LastUpdated = DateTime.UtcNow;
                    _syncOrderRepository.SaveChanges();
                }

                shopifyCustomerRecord.IsUpdatedInAcumatica = true;
                transaction.Commit();

                return output;
            }
        }
    }
}
