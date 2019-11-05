using System;
using Monster.Acumatica.Api;
using Monster.Acumatica.Api.Common;
using Monster.Acumatica.Api.Customer;
using Monster.Acumatica.Api.SalesOrder;
using Monster.Middle.Misc.Logging;
using Monster.Middle.Persist.Instance;
using Monster.Middle.Processes.Acumatica.Persist;
using Monster.Middle.Processes.Sync.Model.Misc;
using Monster.Middle.Processes.Sync.Model.Orders;
using Monster.Middle.Processes.Sync.Persist;
using Push.Foundation.Utilities.Json;


namespace Monster.Middle.Processes.Sync.Workers
{
    public class AcumaticaCustomerPut
    {
        private readonly AcumaticaOrderRepository _acumaticaOrderRepository;
        private readonly SyncOrderRepository _syncOrderRepository;
        private readonly CustomerClient _customerClient;
        private readonly ExecutionLogService _logService;
        private readonly SettingsRepository _settingsRepository;


        public AcumaticaCustomerPut(
                AcumaticaOrderRepository acumaticaOrderRepository,
                SyncOrderRepository syncOrderRepository, 
                CustomerClient customerClient, 
                ExecutionLogService logService, 
                SettingsRepository settingsRepository)
        {
            _acumaticaOrderRepository = acumaticaOrderRepository;
            _syncOrderRepository = syncOrderRepository;
            _customerClient = customerClient;
            _logService = logService;
            _settingsRepository = settingsRepository;
        }

        public void Run()
        {
            var notLoadedCustomers = _syncOrderRepository.RetrieveShopifyCustomersWithoutSyncs();

            foreach (var shopifyCustomer in notLoadedCustomers)
            {
                var msg = LogBuilder.CreateAcumaticaCustomer(shopifyCustomer);
                _logService.Log(msg);
                PushCustomer(shopifyCustomer);
            }

            var customersNeedingUpdate = _syncOrderRepository.RetrieveShopifyCustomersNeedingPut();

            foreach (var shopifyCustomer in customersNeedingUpdate)
            {
                var msg = LogBuilder.UpdateAcumaticaCustomer(shopifyCustomer);
                _logService.Log(msg);
                PushCustomer(shopifyCustomer);
            }
        }

        public AcumaticaCustomer PushCustomer(ShopifyCustomer shopifyCustomerRecord)
        {
            var shopifyCustomer =
                shopifyCustomerRecord
                    .ShopifyJson.DeserializeFromJson<Push.Shopify.Api.Customer.Customer>();

            // Push Customer to Acumatica API
            //
            var customer = BuildCustomer(shopifyCustomer);
            var acumaticaCustomer = _customerClient.WriteCustomer(customer);
            

            // Create SQL footprint in Monster
            //
            using (var transaction = _syncOrderRepository.BeginTransaction())
            {
                // Create the local cache of Acumatica Customer record
                //
                var acumaticaCustomerRecord 
                        = UpsertCustomerToPersist(acumaticaCustomer, shopifyCustomerRecord);

                // Lastly, flag the Shopify Customer as updated
                //
                shopifyCustomerRecord.NeedsCustomerPut = false;
                _syncOrderRepository.SaveChanges();
                transaction.Commit();

                return acumaticaCustomerRecord;
            }
        }


        // *** CRITICAL - only call this method for Customers that have been intentionally
        // ... written to Acumatica
        //
        private AcumaticaCustomer 
                UpsertCustomerToPersist(Customer acumaticaCustomer, ShopifyCustomer shopifyCustomer)
        {
            var existingRecord =
                _acumaticaOrderRepository.RetrieveCustomer(acumaticaCustomer.CustomerID.value);

            if (existingRecord != null)
            {
                existingRecord.AcumaticaJson = acumaticaCustomer.SerializeToJson();
                existingRecord.AcumaticaMainContactEmail = acumaticaCustomer.MainContact.Email.value;
                existingRecord.LastUpdated = DateTime.UtcNow;
                _acumaticaOrderRepository.SaveChanges();
                return existingRecord;
            }
            else
            {
                var newRecord = new AcumaticaCustomer();

                newRecord.AcumaticaCustomerId = acumaticaCustomer.CustomerID.value;
                newRecord.AcumaticaJson = acumaticaCustomer.SerializeToJson();
                newRecord.AcumaticaMainContactEmail = acumaticaCustomer.MainContact.Email.value;
                newRecord.ShopifyCustomer = shopifyCustomer;
                newRecord.DateCreated = DateTime.UtcNow;
                newRecord.LastUpdated = DateTime.UtcNow;

                _acumaticaOrderRepository.InsertCustomer(newRecord);
                return newRecord;
            }
        }

        private Customer BuildCustomer(Push.Shopify.Api.Customer.Customer shopifyCustomer)
        {
            var name = shopifyCustomer.first_name + " " + shopifyCustomer.last_name;
            var settings = _settingsRepository.RetrieveSettings();

            var customer = new Customer();
            customer.CustomerID = shopifyCustomer.id.ToString().ToValue();
            customer.CustomerName = name.ToValue();
            customer.TaxZone = settings.AcumaticaTaxZone.ToValue();

            var address = new Address();
            if (shopifyCustomer.default_address != null)
            {
                var shopifyAddress = shopifyCustomer.default_address;

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

