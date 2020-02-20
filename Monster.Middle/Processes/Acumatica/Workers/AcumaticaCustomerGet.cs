using System;
using System.Collections.Generic;
using Monster.Acumatica.Api;
using Monster.Acumatica.Api.Customer;
using Monster.Acumatica.Config;
using Monster.Middle.Misc.Acumatica;
using Monster.Middle.Misc.Hangfire;
using Monster.Middle.Processes.Acumatica.Persist;
using Monster.Middle.Processes.Sync.Persist;
using Push.Foundation.Utilities.Json;


namespace Monster.Middle.Processes.Acumatica.Workers
{
    public class AcumaticaCustomerGet
    {
        private readonly CustomerClient _customerClient;
        private readonly AcumaticaBatchRepository _batchStateRepository;
        private readonly AcumaticaOrderRepository _orderRepository;
        private readonly AcumaticaTimeZoneService _instanceTimeZoneService;
        private readonly SettingsRepository _settingsRepository;
        private readonly JobMonitoringService _jobMonitoringService;
        private readonly AcumaticaHttpConfig _config;
        private readonly AcumaticaJsonService _acumaticaJsonService;

        public AcumaticaCustomerGet(
                CustomerClient customerClient,
                AcumaticaOrderRepository orderRepository,
                AcumaticaBatchRepository batchStateRepository,
                AcumaticaTimeZoneService instanceTimeZoneService,
                SettingsRepository settingsRepository,
                JobMonitoringService jobMonitoringService,
                AcumaticaHttpConfig config, AcumaticaJsonService acumaticaJsonService)
        {
            _customerClient = customerClient;
            _orderRepository = orderRepository;
            _batchStateRepository = batchStateRepository;
            _instanceTimeZoneService = instanceTimeZoneService;
            _settingsRepository = settingsRepository;
            _jobMonitoringService = jobMonitoringService;
            _config = config;
            _acumaticaJsonService = acumaticaJsonService;
        }


        public void RunAutomatic()
        {
            var batchState = _batchStateRepository.Retrieve();
            var settings = _settingsRepository.RetrieveSettings();

            if (batchState.AcumaticaCustomersGetEnd.HasValue)
            {
                RunWithPaging(batchState.AcumaticaCustomersGetEnd.Value);
            }
            else
            {
                RunWithPaging(settings.ShopifyOrderCreatedAtUtc.Value);
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
                if (_jobMonitoringService.DetectCurrentJobInterrupt())
                {
                    return;
                }

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
                using (var transaction = _orderRepository.BeginTransaction())
                {
                    _acumaticaJsonService.Upsert(
                        AcumaticaJsonType.Customer,
                        acumaticaCustomer.CustomerID.value,
                        acumaticaCustomer.SerializeToJson());
                    existingRecord.AcumaticaMainContactEmail = acumaticaCustomer.MainContact.Email.value;
                    existingRecord.LastUpdated = DateTime.UtcNow;
                    _orderRepository.SaveChanges();
                    transaction.Commit();
                }
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

