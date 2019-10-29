using System;
using System.Collections.Generic;
using Monster.Acumatica.Api;
using Monster.Acumatica.Api.Customer;
using Monster.Acumatica.Config;
using Monster.Middle.Processes.Acumatica.Persist;
using Monster.Middle.Processes.Sync.Model.Status;
using Monster.Middle.Processes.Sync.Persist;
using Monster.Middle.Utility;
using Push.Foundation.Utilities.Json;


namespace Monster.Middle.Processes.Acumatica.Workers
{
    public class AcumaticaCustomerGet
    {
        private readonly CustomerClient _customerClient;
        private readonly AcumaticaBatchRepository _batchStateRepository;
        private readonly AcumaticaOrderRepository _orderRepository;
        private readonly AcumaticaTimeZoneService _instanceTimeZoneService;
        private readonly PreferencesRepository _preferencesRepository;
        private readonly AcumaticaHttpConfig _config;


        public AcumaticaCustomerGet(
                CustomerClient customerClient,
                AcumaticaOrderRepository orderRepository,
                AcumaticaBatchRepository batchStateRepository,
                AcumaticaTimeZoneService instanceTimeZoneService,
                PreferencesRepository preferencesRepository,
                AcumaticaHttpConfig config)
        {
            _customerClient = customerClient;
            _orderRepository = orderRepository;
            _batchStateRepository = batchStateRepository;
            _instanceTimeZoneService = instanceTimeZoneService;
            _preferencesRepository = preferencesRepository;
            _config = config;
        }


        public void RunAutomatic()
        {
            var batchState = _batchStateRepository.Retrieve();
            var preferences = _preferencesRepository.RetrievePreferences();
            preferences.AssertStartingOrderIsValid();

            if (batchState.AcumaticaCustomersGetEnd.HasValue)
            {
                RunWithPaging(batchState.AcumaticaCustomersGetEnd.Value);
            }
            else
            {
                RunWithPaging(preferences.ShopifyOrderCreatedAtUtc.Value);
            }
        }

        private void RunWithPaging(DateTime updateMinUtc)
        {
            var startOfRun = DateTime.UtcNow;
            var page = 1;
            var pageSize = _config.PageSize;

            var updateMin = _instanceTimeZoneService.ToAcumaticaTimeZone(updateMinUtc);

            while (true)
            {
                var json = _customerClient.RetrieveCustomers(updateMin, page, pageSize);
                var customers = json.DeserializeFromJson<List<Customer>>();

                if (customers.Count == 0)
                {
                    break;
                }

                foreach (var customer in customers)
                {
                    UpdateCustomerToPersist(customer);

                    // Disaster recovery
                    // AttemptToLinkCustomerWithExistingRecord()
                }

                page++;
            }

            var GetEnd = (startOfRun).AddAcumaticaBatchFudge();
            _batchStateRepository.UpdateCustomersGetEnd(GetEnd);
        }

        private void UpdateCustomerToPersist(Customer acumaticaCustomer)
        {
            var existingRecord = _orderRepository.RetrieveCustomer(acumaticaCustomer.CustomerID.value);

            if (existingRecord != null)
            {
                existingRecord.AcumaticaJson = acumaticaCustomer.SerializeToJson();
                existingRecord.AcumaticaMainContactEmail = acumaticaCustomer.MainContact.Email.value;
                existingRecord.LastUpdated = DateTime.UtcNow;
                _orderRepository.SaveChanges();
            }
        }


        //// *** Pending Disaster Recovery
        ////
        //public long RunAndUpsertCustomerIfNotExists(string acumaticaCustomerId)
        //{
        //    var existingCustomer
        //            = _orderRepository.RetrieveCustomer(acumaticaCustomerId);

        //    if (existingCustomer == null)
        //    {
        //        var customerJson = _customerClient.RetrieveCustomer(acumaticaCustomerId);
        //        var customer = customerJson.DeserializeFromJson<Customer>();
        //        var newData = customer.ToMonsterRecord();

        //        _orderRepository.InsertCustomer(newData);
        //        return newData.Id;
        //    }
        //    else
        //    {
        //        return existingCustomer.Id;
        //    }
        //}
    }
}

