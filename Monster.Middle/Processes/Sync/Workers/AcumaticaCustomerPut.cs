using System;
using System.Linq;
using Monster.Acumatica.Api;
using Monster.Acumatica.Api.Common;
using Monster.Acumatica.Api.Customer;
using Monster.Acumatica.Api.SalesOrder;
using Monster.Middle.Misc.Logging;
using Monster.Middle.Persist.Instance;
using Monster.Middle.Processes.Acumatica.Persist;
using Monster.Middle.Processes.Acumatica.Workers;
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

        public AcumaticaCustomerPut(
                AcumaticaOrderRepository acumaticaOrderRepository,
                SyncOrderRepository syncOrderRepository, 
                CustomerClient customerClient, 
                ExecutionLogService logService)
        {
            _acumaticaOrderRepository = acumaticaOrderRepository;
            _syncOrderRepository = syncOrderRepository;
            _customerClient = customerClient;
            _logService = logService;
        }

        public void Run()
        {
            var notLoadedCustomers = _syncOrderRepository.RetrieveUnsyncedShopifyCustomers();

            foreach (var shopifyCustomer in notLoadedCustomers)
            {
                var msg = LogBuilder.CreateAcumaticaCustomer(shopifyCustomer);
                _logService.Log(msg);
                PushCustomer(shopifyCustomer);
            }

            var customersNeedingUpdate = _syncOrderRepository.RetrieveCustomersNeedingUpdate();

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
                var acumaticaCustomerRecord = UpsertCustomerToPersist(acumaticaCustomer);

                // Create the Sync record between Shopify and Acumatica Customer records
                //
                UpsertCustomerSync(shopifyCustomerRecord, acumaticaCustomerRecord);

                // Lastly, flag the Shopify Customer as updated
                //
                shopifyCustomerRecord.IsUpdatedInAcumatica = true;
                _syncOrderRepository.SaveChanges();
                transaction.Commit();

                return acumaticaCustomerRecord;
            }
        }


        // *** CRITICAL - this method should only be invoked in the event of intentionally
        // ... pushing a Shopify Customer to Acumatica
        //
        private AcumaticaCustomer UpsertCustomerToPersist(Customer acumaticaCustomer)
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
                var newRecord = acumaticaCustomer.ToMonsterRecord();
                _acumaticaOrderRepository.InsertCustomer(newRecord);
                return newRecord;
            }
        }

        private void UpsertCustomerSync(
                ShopifyCustomer shopifyCustomerRecord, AcumaticaCustomer acumaticaCustomerRecord)
        {
            if (!shopifyCustomerRecord.HasMatch())
            {
                var syncRecord = new ShopAcuCustomerSync();
                syncRecord.ShopifyCustomer = shopifyCustomerRecord;
                syncRecord.AcumaticaCustomer = acumaticaCustomerRecord;
                syncRecord.DateCreated = DateTime.UtcNow;
                _syncOrderRepository.InsertCustomerSync(syncRecord);
            }
        }


        private static Customer BuildCustomer(Push.Shopify.Api.Customer.Customer shopifyCustomer)
        {
            var name = shopifyCustomer.first_name + " " + shopifyCustomer.last_name;
            var shopifyAddress = shopifyCustomer.default_address;

            var customer = new Customer();
            customer.CustomerID = shopifyCustomer.id.ToString().ToValue();
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

