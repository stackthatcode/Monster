using System;
using System.Collections.Generic;
using Monster.Middle.Persist.Multitenant;
using Monster.Middle.Processes.Shopify.Persist;
using Push.Foundation.Utilities.Json;
using Push.Foundation.Utilities.Logging;
using Push.Shopify.Api;
using Push.Shopify.Api.Customer;

namespace Monster.Middle.Processes.Shopify.Workers
{
    public class ShopifyCustomerPull
    {
        private readonly CustomerApi _customerApi;
        private readonly ShopifyOrderRepository _orderRepository;
        private readonly ShopifyBatchRepository _shopifyBatchRepository;
        private readonly ConnectionRepository _connectionRepository;
        private readonly IPushLogger _logger;
        private readonly PreferencesRepository _preferencesRepository;

        // Possibly expand - this is a one-time thing...
        //
        public const int InitialBatchStateFudgeMin = -15;

        public ShopifyCustomerPull(
                CustomerApi customerApi,
                ShopifyOrderRepository orderRepository,
                ShopifyBatchRepository shopifyBatchRepository,
                ConnectionRepository connectionRepository,
                PreferencesRepository preferencesRepository,
                IPushLogger logger)
        {
            _customerApi = customerApi;
            _orderRepository = orderRepository;
            _shopifyBatchRepository = shopifyBatchRepository;
            _connectionRepository = connectionRepository;
            _logger = logger;
            _preferencesRepository = preferencesRepository;
        }


        public void RunAutomatic()
        {
            var batchState = _shopifyBatchRepository.Retrieve();
            if (batchState.ShopifyCustomersPullEnd.HasValue)
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
            _logger.Debug("ShopifyCustomerPull -> RunAll()");

            var startOfPullRun = DateTime.UtcNow;
            var preferences = _preferencesRepository.RetrievePreferences();

            var firstFilter = new SearchFilter();
            firstFilter.CreatedAtMinUtc = preferences.ShopifyOrderDateStart;

            var firstJson = _customerApi.Retrieve(firstFilter);
            var firstCustomers 
                = firstJson.DeserializeFromJson<CustomerList>().customers;

            UpsertCustomers(firstCustomers);

            var currentPage = 2;

            while (true)
            {
                var currentFilter = firstFilter.Clone();
                currentFilter.Page = currentPage;
                var currentJson = _customerApi.Retrieve(currentFilter);
                var currentCustomers 
                    = currentJson.DeserializeFromJson<CustomerList>().customers;

                if (currentCustomers.Count == 0)
                {
                    break;
                }

                UpsertCustomers(currentCustomers);

                currentPage++;
            }

            // Compute the Batch State end marker
            var maxUpdatedDate =
                _orderRepository.RetrieveCustomerMaxUpdatedDate();

            var orderBatchEnd
                = maxUpdatedDate
                  ?? DateTime.UtcNow.AddMinutes(InitialBatchStateFudgeMin);

            _shopifyBatchRepository
                .UpdateShopifyCustomersPullEnd(orderBatchEnd);
        }

        private void RunUpdated()
        {
            _logger.Debug("ShopifyCustomerPull -> RunUpdated()");

            var batchState = _shopifyBatchRepository.Retrieve();

            if (!batchState.ShopifyCustomersPullEnd.HasValue)
            {
                throw new Exception(
                    "ShopifyCustomersPullEnd not set - must run Baseline Pull first");
            }

            var lastBatchStateEnd = batchState.ShopifyCustomersPullEnd.Value;
            var startOfPullRun = DateTime.UtcNow; // Trick - we won't use this in filtering

            var firstFilter = new SearchFilter();
            firstFilter.Page = 1;
            firstFilter.UpdatedAtMinUtc = lastBatchStateEnd;

            // Pull from Shopify
            var firstJson = _customerApi.Retrieve(firstFilter);
            var firstCustomers = firstJson.DeserializeFromJson<CustomerList>().customers;
            UpsertCustomers(firstCustomers);

            var currentPage = 2;

            while (true)
            {
                var currentFilter = firstFilter.Clone();
                currentFilter.Page = currentPage;

                // Pull from Shopify
                var currentJson = _customerApi.Retrieve(currentFilter);
                var currentCustomers 
                    = currentJson.DeserializeFromJson<CustomerList>().customers;

                UpsertCustomers(currentCustomers);

                if (currentCustomers.Count == 0)
                {
                    break;
                }

                currentPage++;
            }

            _shopifyBatchRepository
                .UpdateShopifyCustomersPullEnd(startOfPullRun);
        }

        private void UpsertCustomers(List<Customer> customers)
        {
            foreach (var customer in customers)
            {
                UpsertCustomer(customer);
            }
        }

        private void UpsertCustomer(Customer customer)
        {
            var existingCustomer 
                = _orderRepository.RetrieveCustomer(customer.id);

            if (existingCustomer == null)
            {
                var newCustomer = new UsrShopifyCustomer();
                newCustomer.ShopifyCustomerId = customer.id;
                newCustomer.ShopifyJson = customer.SerializeToJson();
                newCustomer.ShopifyPrimaryEmail = customer.email;
                newCustomer.DateCreated = DateTime.UtcNow;
                newCustomer.LastUpdated = DateTime.UtcNow;
                newCustomer.IsUpdatedInAcumatica = false;
                _orderRepository.InsertCustomer(newCustomer);                
            }
            else
            {
                existingCustomer.ShopifyJson = customer.SerializeToJson();
                existingCustomer.ShopifyPrimaryEmail = customer.email;
                existingCustomer.IsUpdatedInAcumatica = false;
                existingCustomer.LastUpdated = DateTime.UtcNow;
                
                _orderRepository.SaveChanges();
            }
        }        
    }
}

