using System;
using System.Collections.Generic;
using Monster.Acumatica.Api;
using Monster.Acumatica.Api.Customer;
using Monster.Middle.Persist.Multitenant;
using Monster.Middle.Processes.Acumatica.Persist;
using Monster.Middle.Processes.Sync.Orders.Model;
using Monster.Middle.Services;
using Push.Foundation.Utilities.Json;
using Push.Foundation.Utilities.Logging;

namespace Monster.Middle.Processes.Acumatica.Workers
{
    public class AcumaticaCustomerPull
    {
        private readonly CustomerClient _customerClient;
        private readonly AcumaticaOrderRepository _orderRepository;
        private readonly AcumaticaBatchRepository _batchStateRepository;
        private readonly TimeZoneService _timeZoneService;
        private readonly ConnectionRepository _tenantRepository;
        private readonly IPushLogger _logger;

        public const int InitialBatchStateFudgeMin = -15;


        public AcumaticaCustomerPull(
                CustomerClient customerClient,
                AcumaticaOrderRepository orderRepository,
                AcumaticaBatchRepository batchStateRepository,
                TimeZoneService timeZoneService,
                ConnectionRepository tenantRepository,
                IPushLogger logger)
        {
            _customerClient = customerClient;
            _orderRepository = orderRepository;
            _batchStateRepository = batchStateRepository;
            _timeZoneService = timeZoneService;
            _tenantRepository = tenantRepository;
            _logger = logger;
        }


        public void RunAutomatic()
        {
            var batchState = _batchStateRepository.Retrieve();
            if (batchState.AcumaticaCustomersPullEnd.HasValue)
            {
                RunUpdated();
            }
            else
            {
                RunAll();
            }
        }

        private void RunAll()
        {
            var preferences = _tenantRepository.RetrievePreferences();
            var customerUpdateMin = preferences.ShopifyOrderDateStart;
            
            var json = _customerClient.RetrieveCustomers(customerUpdateMin);
            var customers = json.DeserializeFromJson<List<Customer>>();

            UpsertCustomersToPersist(customers);

            // Set the Batch State Pull End marker
            var maxCustomerDate =
                _orderRepository
                    .RetrieveCustomerMaxUpdatedDate();

            var batchStateEnd 
                = maxCustomerDate 
                    ?? DateTime.UtcNow.AddMinutes(InitialBatchStateFudgeMin);

            _batchStateRepository.UpdateCustomersPullEnd(batchStateEnd);
        }

        private void RunUpdated()
        {
            var batchState = _batchStateRepository.Retrieve();
            if (!batchState.AcumaticaCustomersPullEnd.HasValue)
            {
                throw new Exception(
                    "AcumaticaCustomersPullEnd is null - execute RunAll() first");
            }

            var updateMinUtc = batchState.AcumaticaCustomersPullEnd;
            var updateMin = _timeZoneService.ToAcumaticaTimeZone(updateMinUtc.Value);


            var pullRunStartTime = DateTime.UtcNow;

            var json = _customerClient.RetrieveCustomers(updateMin);
            var customers = json.DeserializeFromJson<List<Customer>>();
            UpsertCustomersToPersist(customers);

            _batchStateRepository.UpdateCustomersPullEnd(pullRunStartTime);
        }


        public void UpsertCustomersToPersist(List<Customer> customers)
        {
            foreach (var customer in customers)
            {
                UpsertCustomerToPersist(customer);
            }
        }

        public UsrAcumaticaCustomer UpsertCustomerToPersist(Customer customer)
        {
            var existingData
                = _orderRepository
                    .RetrieveCustomer(customer.CustomerID.value);

            if (existingData == null)
            {
                var newData = customer.ToMonsterRecord();
                _orderRepository.InsertCustomer(newData);
                return newData;
            }
            else
            {
                existingData.AcumaticaJson = customer.SerializeToJson();
                existingData.AcumaticaMainContactEmail = customer.MainContact.Email.value;
                existingData.LastUpdated = DateTime.UtcNow;

                _orderRepository.SaveChanges();
                return existingData;
            }
        }
        
        public long RunAndUpsertCustomer(string acumaticaCustomerId)
        {
            var existingCustomer
                = _orderRepository.RetrieveCustomer(acumaticaCustomerId);

            if (existingCustomer == null)
            {
                var customerJson
                    = _customerClient.RetrieveCustomer(acumaticaCustomerId);

                var customer = customerJson.DeserializeFromJson<Customer>();

                var newData = customer.ToMonsterRecord();
                _orderRepository.InsertCustomer(newData);

                return newData.Id;
            }
            else
            {
                return existingCustomer.Id;
            }
        }
    }
}

