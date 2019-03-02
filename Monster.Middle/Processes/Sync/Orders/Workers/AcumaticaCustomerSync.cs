using System;
using System.Linq;
using Monster.Acumatica.Api;
using Monster.Acumatica.Api.Common;
using Monster.Acumatica.Api.Customer;
using Monster.Acumatica.Api.SalesOrder;
using Monster.Middle.Persist.Multitenant;
using Monster.Middle.Processes.Acumatica.Workers;
using Monster.Middle.Processes.Sync.Extensions;
using Monster.Middle.Processes.Sync.Orders.Persist;
using Push.Foundation.Utilities.Json;


namespace Monster.Middle.Processes.Sync.Orders.Workers
{
    public class AcumaticaCustomerSync
    {
        private readonly AcumaticaCustomerPull _acumaticaCustomerPull;
        private readonly SyncOrderRepository _syncOrderRepository;
        private readonly CustomerClient _customerClient;
        private readonly ExecutionLogService _executionLogService;

        public AcumaticaCustomerSync(
                AcumaticaCustomerPull acumaticaCustomerPull,
                SyncOrderRepository syncOrderRepository, 
                CustomerClient customerClient, 
                ExecutionLogService executionLogService)
        {
            _acumaticaCustomerPull = acumaticaCustomerPull;
            _syncOrderRepository = syncOrderRepository;
            _customerClient = customerClient;
            _executionLogService = executionLogService;
        }

        public void Run()
        {
            var notLoadedCustomers 
                    = _syncOrderRepository.RetrieveUnsyncedShopifyCustomers();

            foreach (var shopifyCustomer in notLoadedCustomers)
            {
                _executionLogService.RunTransaction(
                    () => PushCustomer(shopifyCustomer),
                    SyncDescriptor.CreateAcumaticaCustomer,
                    SyncDescriptor.ShopifyCustomer(shopifyCustomer));
            }

            var customersNeedingUpdate = _syncOrderRepository.RetrieveCustomersNeedingUpdate();

            foreach (var shopifyCustomer in customersNeedingUpdate)
            {
                _executionLogService.RunTransaction(
                    () => PushCustomer(shopifyCustomer),
                    SyncDescriptor.UpdateAcumaticaCustomer,
                    SyncDescriptor.ShopifyCustomer(shopifyCustomer));
            }
        }


        public UsrAcumaticaCustomer PushCustomer(UsrShopifyCustomer shopifyCustomerRecord)
        {
            var shopifyCustomer =
                shopifyCustomerRecord
                    .ShopifyJson
                    .DeserializeFromJson<Push.Shopify.Api.Customer.Customer>();
            
            var customer = BuildCustomer(shopifyCustomer);

            var customerRecord = shopifyCustomerRecord.Match();
            if (customerRecord != null)
            {
                customer.CustomerID = customerRecord.AcumaticaCustomerId.ToValue();
            }

            // Push Customer to Acumatica API
            var resultJson = _customerClient.WriteCustomer(customer.SerializeToJson());
            var customerResult = resultJson.DeserializeFromJson<Customer>();

            var log = $"Wrote Customer {customerResult.CustomerID.value} to Acumatica";
            _executionLogService.InsertExecutionLog(log);
            
            // Create SQL footprint
            using (var transaction = _syncOrderRepository.BeginTransaction())
            {
                var output = _acumaticaCustomerPull.UpsertCustomerToPersist(customerResult);
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
                _syncOrderRepository.SaveChanges();
                transaction.Commit();

                return output;
            }
        }


        private static Customer BuildCustomer(Push.Shopify.Api.Customer.Customer shopifyCustomer)
        {
            var name = shopifyCustomer.first_name + " " + shopifyCustomer.last_name;
            var shopifyAddress = shopifyCustomer.default_address;

            var customer = new Customer();
            customer.CustomerName = name.ToValue();

            var address = new Address();
            if (shopifyAddress != null)
            {
                address.AddressLine1 = shopifyAddress.address1.ToValue();
                address.AddressLine2 = shopifyAddress.address2.ToValue();
                address.City = shopifyAddress.city.ToValue();
                address.State = shopifyAddress.province.ToValue();
                address.PostalCode = shopifyAddress.zip.ToValue();
            }

            var mainContact = new Contact();
            mainContact.Address = address;
            mainContact.FirstName = shopifyCustomer.first_name.ToValue();
            mainContact.LastName = shopifyCustomer.last_name.ToValue();
            mainContact.Phone1 = shopifyCustomer.phone.ToValue();
            mainContact.Email = shopifyCustomer.email.ToValue();

            customer.MainContact = mainContact;
            customer.AccountRef = $"Shopify Customer #{shopifyCustomer.id}".ToValue();
            return customer;
        }
    }
}

