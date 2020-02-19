using System;
using System.Collections.Generic;
using System.Linq;
using Monster.Acumatica.Api;
using Monster.Acumatica.Api.Common;
using Monster.Acumatica.Api.Customer;
using Monster.Acumatica.Api.SalesOrder;
using Monster.Middle.Misc.Hangfire;
using Monster.Middle.Misc.Logging;
using Monster.Middle.Persist.Instance;
using Monster.Middle.Processes.Acumatica.Persist;
using Monster.Middle.Processes.Shopify.Persist;
using Monster.Middle.Processes.Sync.Misc;
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
        private readonly JobMonitoringService _jobMonitoringService;
        private readonly AcumaticaJsonService _acumaticaJsonService;
        private readonly ShopifyJsonService _shopifyJsonService;
        private readonly SettingsRepository _settingsRepository;
        private readonly ExecutionLogService _logService;


        public AcumaticaCustomerPut(
                AcumaticaOrderRepository acumaticaOrderRepository,
                SyncOrderRepository syncOrderRepository, 
                CustomerClient customerClient, 
                JobMonitoringService jobMonitoringService,
                ExecutionLogService logService, 
                SettingsRepository settingsRepository, AcumaticaJsonService acumaticaJsonService)
        {
            _acumaticaOrderRepository = acumaticaOrderRepository;
            _syncOrderRepository = syncOrderRepository;
            _customerClient = customerClient;
            _jobMonitoringService = jobMonitoringService;
            _logService = logService;
            _settingsRepository = settingsRepository;
            _acumaticaJsonService = acumaticaJsonService;
        }

        public void Run()
        {
            var notLoadedCustomers = _syncOrderRepository.RetrieveShopifyCustomersWithoutSyncs();

            foreach (var shopifyCustomer in notLoadedCustomers)
            {
                if (_jobMonitoringService.DetectCurrentJobInterrupt())
                {
                    return;
                }
                PushCustomer(shopifyCustomer);
            }

            var customersNeedingUpdate = _syncOrderRepository.RetrieveShopifyCustomersNeedingPut();

            foreach (var shopifyCustomer in customersNeedingUpdate)
            {
                if (_jobMonitoringService.DetectCurrentJobInterrupt())
                {
                    return;
                }

                PushCustomer(shopifyCustomer);
            }
        }

        public AcumaticaCustomer PushCustomer(ShopifyCustomer shopifyRecord)
        {
            if (shopifyRecord.HasMatch())
            {
                // Already matched - push updates to Acumatica
                //
                UpdateCustomerInAcumatica(shopifyRecord);
                return shopifyRecord.AcumaticaCustomer;
            }
            else
            { 
                // No matching record
                //
                var customerInAcumatica = FindAcumaticaCustomer(shopifyRecord);

                if (customerInAcumatica == null)
                {
                    // Unable to locate Customer in Acumatica? Push brand new Customer to Acumatica API
                    //
                    _logService.Log(LogBuilder.CreateAcumaticaCustomer(shopifyRecord));
                    var newCustomer = BuildCustomer(shopifyRecord);
                    var newCustomerResult = _customerClient.WriteCustomer(newCustomer);

                    // Then create a SQL record thereof
                    //
                    var acumaticaRecord = CreateAcumaticaCustomerRecord(shopifyRecord, newCustomerResult);

                    return acumaticaRecord;
                }
                else
                {
                    // Found Customer in Acumatica! Create a SQL record for it...
                    // 
                    _logService.Log(LogBuilder.AutomatchingCustomers(shopifyRecord, customerInAcumatica));
                    var acumaticaRecord = CreateAcumaticaCustomerRecord(shopifyRecord, customerInAcumatica);

                    // ... and then now push an update from Shopify into Acumatica
                    // TODO - make this update in Acumatica optional
                    //
                    UpdateCustomerInAcumatica(shopifyRecord);

                    return acumaticaRecord;
                }
            }
        }

        public AcumaticaCustomer CreateAcumaticaCustomerRecord(ShopifyCustomer shopifyRecord, Customer acumaticaCustomer)
        {
            using (var transaction = _acumaticaOrderRepository.BeginTransaction())
            {
                var newRecord = new AcumaticaCustomer();

                newRecord.AcumaticaCustomerId = acumaticaCustomer.CustomerID.value;
                newRecord.AcumaticaMainContactEmail = acumaticaCustomer.MainContact.Email.value;
                newRecord.DateCreated = DateTime.UtcNow;
                newRecord.LastUpdated = DateTime.UtcNow;

                shopifyRecord.AcumaticaCustomer = newRecord;
                shopifyRecord.NeedsCustomerPut = false;

                _acumaticaOrderRepository.InsertCustomer(newRecord);

                _acumaticaJsonService.Upsert(
                    AcumaticaJsonType.Customer, 
                    acumaticaCustomer.CustomerID.value,
                    acumaticaCustomer.SerializeToJson());

                transaction.Commit();
                return newRecord;
            }
        }


        public void UpdateCustomerInAcumatica(ShopifyCustomer shopifyRecord)
        {
            _logService.Log(LogBuilder.UpdateAcumaticaCustomer(shopifyRecord));

            using (var transaction = _acumaticaOrderRepository.BeginTransaction())
            {
                var acumaticaCustomer = BuildCustomer(shopifyRecord);
                var result = _customerClient.WriteCustomer(acumaticaCustomer);

                var acumaticaRecord = shopifyRecord.AcumaticaCustomer;
                acumaticaRecord.AcumaticaMainContactEmail = acumaticaCustomer.MainContact.Email.value;
                acumaticaRecord.LastUpdated = DateTime.UtcNow;

                shopifyRecord.NeedsCustomerPut = false;
                _syncOrderRepository.SaveChanges();

                _acumaticaJsonService.Upsert(
                    AcumaticaJsonType.Customer, acumaticaRecord.AcumaticaCustomerId, result.SerializeToJson());
                transaction.Commit();
            }
        }


        public Customer FindAcumaticaCustomer(ShopifyCustomer shopifyCustomer)
        {
            var customersByIdJson = _customerClient.SearchCustomerByCustomerId(shopifyCustomer.ShopifyCustomerId.ToString());
            var customersById = customersByIdJson.DeserializeFromJson<List<Customer>>();
            if (customersById.Count == 1)
            {
                return customersById.First();
            }

            var customersByEmailJson = _customerClient.SearchCustomerByEmail(shopifyCustomer.ShopifyPrimaryEmail);
            var customers = customersByEmailJson.DeserializeFromJson<List<Customer>>();
            if (customers.Count == 0)
            {
                return null;
            }
            if (customers.Count == 1)
            {
                _logService.Log(LogBuilder.FoundAcumaticaCustomerByEmail(shopifyCustomer.ShopifyPrimaryEmail));
            }
            else
            {
                _logService.Log(LogBuilder.MultipleCustomersWithSameEmail(shopifyCustomer.ShopifyPrimaryEmail));
            }

            return customers.First();
        }

        private Customer BuildCustomer(ShopifyCustomer customerRecord)
        {
            var shopifyCustomer = _shopifyJsonService.RetrieveCustomer(customerRecord.ShopifyCustomerId);

            var name = shopifyCustomer.first_name + " " + shopifyCustomer.last_name;
            var settings = _settingsRepository.RetrieveSettings();

            var customer = new Customer();
            var newAcumaticaCustomerId
                = customerRecord.AcumaticaCustomer == null
                    ? customerRecord.ShopifyCustomerId.ToString()
                    : customerRecord.AcumaticaCustomer.AcumaticaCustomerId;

            customer.CustomerID = newAcumaticaCustomerId.ToValue();
            customer.CustomerName = name.ToValue();
            //customer.TaxZone = settings.AcumaticaTaxZone.ToValue();

            var address = new Address();
            if (shopifyCustomer.default_address != null)
            {
                var shopifyAddress = shopifyCustomer.default_address;

                address.AddressLine1 = shopifyAddress.address1.ToValue();
                address.AddressLine2 = shopifyAddress.address2.ToValue();
                address.City = shopifyAddress.city.ToValue();
                address.State = shopifyAddress.province.ToValue();
                address.PostalCode = shopifyAddress.zip.ToValue();
                address.CountryID = shopifyAddress.country_code.ToValue();
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

