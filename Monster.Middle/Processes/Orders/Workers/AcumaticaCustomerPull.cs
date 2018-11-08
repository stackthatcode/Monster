using System;
using System.Collections.Generic;
using Monster.Acumatica.Api;
using Monster.Acumatica.Api.Customer;
using Monster.Middle.Persist.Multitenant;
using Monster.Middle.Persist.Multitenant.Acumatica;
using Monster.Middle.Persist.Multitenant.Etc;
using Push.Foundation.Utilities.Json;
using Push.Foundation.Utilities.Logging;


namespace Monster.Middle.Processes.Orders.Workers
{
    public class AcumaticaCustomerPull
    {
        private readonly CustomerClient _customerClient;
        private readonly AcumaticaOrderRepository _orderRepository;
        private readonly BatchStateRepository _batchStateRepository;
        private readonly TenantRepository _tenantRepository;
        private readonly IPushLogger _logger;

        public const int InitialBatchStateFudgeMin = -15;


        public AcumaticaCustomerPull(
                CustomerClient customerClient,
                AcumaticaOrderRepository orderRepository,
                BatchStateRepository batchStateRepository,
                TenantRepository tenantRepository,
                IPushLogger logger)
        {
            _customerClient = customerClient;
            _orderRepository = orderRepository;
            _batchStateRepository = batchStateRepository;
            _tenantRepository = tenantRepository;
            _logger = logger;
        }


        public void RunAll()
        {
            var preferences = _tenantRepository.RetrievePreferences();
            var customerUpdateMin = preferences.DataPullStart;
            
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

            _batchStateRepository
                .UpdateAcumaticaCustomersPullEnd(batchStateEnd);
        }
        
        public void UpsertCustomersToPersist(List<Customer> customers)
        {
            foreach (var customer in customers)
            {
                var existingData
                    = _orderRepository
                        .RetrieveCustomer(customer.CustomerID.value);

                if (existingData == null)
                {
                    var newData = customer.ToMonsterRecord();
                    _orderRepository.InsertCustomer(newData);
                }
                else
                {
                    existingData.AcumaticaJson = customer.SerializeToJson();
                    existingData.AcumaticaMainContactEmail
                                = customer.MainContact.Email.value;
                    existingData.LastUpdated = DateTime.UtcNow;

                    _orderRepository.SaveChanges();
                }
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

