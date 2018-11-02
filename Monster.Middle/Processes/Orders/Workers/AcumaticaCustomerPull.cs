using System;
using System.Collections.Generic;
using Monster.Acumatica.Api;
using Monster.Acumatica.Api.Customer;
using Monster.Middle.Persist.Multitenant;
using Push.Foundation.Utilities.Json;
using Push.Foundation.Utilities.Logging;


namespace Monster.Middle.Processes.Orders.Workers
{
    public class AcumaticaCustomerPull
    {
        private readonly CustomerClient _customerClient;
        private readonly OrderRepository _orderRepository;
        private readonly BatchStateRepository _batchStateRepository;
        private readonly TenantRepository _tenantRepository;
        private readonly IPushLogger _logger;

        public const int InitialBatchStateFudgeMin = -15;


        public AcumaticaCustomerPull(
                CustomerClient customerClient, 
                OrderRepository orderRepository,
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
                    .RetrieveShopifyCustomerMaxUpdatedDate();

            var batchStateEnd 
                = maxCustomerDate 
                    ?? DateTime.UtcNow.AddMinutes(InitialBatchStateFudgeMin);

            _batchStateRepository
                .UpdateAcumaticaCustomersPullEnd(batchStateEnd);
        }
        
        public void RunUpdated()
        {
            var batchState = _batchStateRepository.RetrieveBatchState();
            if (!batchState.AcumaticaCustomersPullEnd.HasValue)
            {
                throw new Exception(
                    "AcumaticaCustomersPullEnd is null - execute RunAll() first");
            }

            var customerUpdateMin = batchState.AcumaticaCustomersPullEnd;
            var pullRunStartTime = DateTime.UtcNow;

            var json = _customerClient.RetrieveCustomers(customerUpdateMin);
            var customers = json.DeserializeFromJson<List<Customer>>();

            UpsertCustomersToPersist(customers);

            _batchStateRepository.UpdateAcumaticaCustomersPullEnd(pullRunStartTime);
        }

        public void UpsertCustomersToPersist(List<Customer> customers)
        {
            foreach (var customer in customers)
            {
                var existingData
                    = _orderRepository
                        .RetrieveAcumaticaCustomer(customer.CustomerID.value);

                if (existingData == null)
                {
                    var newData = new UsrAcumaticaCustomer()
                    {
                        AcumaticaCustomerId = customer.CustomerID.value,
                        AcumaticaJson = customer.SerializeToJson(),
                        DateCreated = DateTime.UtcNow,
                        LastUpdated = DateTime.UtcNow,
                    };

                    _orderRepository.InsertAcumaticaCustomer(newData);
                }
                else
                {
                    existingData.AcumaticaJson = customer.SerializeToJson();
                    existingData.LastUpdated = DateTime.UtcNow;

                    _orderRepository.SaveChanges();
                }
            }
        }
    }
}

