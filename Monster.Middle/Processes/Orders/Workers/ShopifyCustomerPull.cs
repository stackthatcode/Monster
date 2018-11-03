using System;
using System.Collections.Generic;
using Monster.Middle.Persist.Multitenant;
using Push.Foundation.Utilities.Json;
using Push.Foundation.Utilities.Logging;
using Push.Shopify.Api;
using Push.Shopify.Api.Customer;

namespace Monster.Middle.Processes.Orders.Workers
{
    public class ShopifyCustomerPull
    {
        private readonly CustomerApi _customerApi;
        private readonly OrderRepository _orderRepository;
        private readonly BatchStateRepository _batchStateRepository;
        private readonly TenantRepository _tenantRepository;
        private readonly IPushLogger _logger;

        // Possibly expand - this is a one-time thing...
        //
        public const int InitialBatchStateFudgeMin = -15;

        public ShopifyCustomerPull(
                CustomerApi customerApi,
                OrderRepository orderRepository,
                BatchStateRepository batchStateRepository,
                TenantRepository tenantRepository,
                IPushLogger logger)
        {
            _customerApi = customerApi;
            _orderRepository = orderRepository;
            _batchStateRepository = batchStateRepository;
            _tenantRepository = tenantRepository;
            _logger = logger;
        }

        public void RunAll()
        {
            _logger.Debug("Baseline Pull Shopify Customer");

            var startOfPullRun = DateTime.UtcNow;
            var preferences = _tenantRepository.RetrievePreferences();

            var firstFilter = new SearchFilter();
            firstFilter.CreatedAtMinUtc = preferences.DataPullStart;

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
                _orderRepository.RetrieveShopifyCustomerMaxUpdatedDate();

            var orderBatchEnd
                = maxUpdatedDate
                  ?? DateTime.UtcNow.AddMinutes(InitialBatchStateFudgeMin);

            _batchStateRepository
                .UpdateShopifyCustomersPullEnd(orderBatchEnd);
        }

        public void RunUpdated()
        {
            var batchState = _batchStateRepository.RetrieveBatchState();

            if (!batchState.ShopifyOrdersPullEnd.HasValue)
            {
                throw new Exception(
                    "ShopifyOrdersPullEnd not set - must run Baseline Pull first");
            }

            var lastBatchStateEnd = batchState.ShopifyOrdersPullEnd.Value;
            var startOfPullRun = DateTime.UtcNow; // Trick - we won't use this in filtering

            var firstFilter = new SearchFilter();
            firstFilter.Page = 1;
            firstFilter.UpdatedAtMinUtc = lastBatchStateEnd;

            // Pull from Shopify
            var firstJson = _customerApi.Retrieve(firstFilter);
            var firstCustomers = firstJson.DeserializeFromJson<CustomerList>().customers;

            var currentPage = 2;

            while (true)
            {
                var currentFilter = firstFilter.Clone();
                currentFilter.Page = currentPage;

                // Pull from Shopify
                var currentJson = _customerApi.Retrieve(currentFilter);
                var currentCustomers 
                    = currentJson.DeserializeFromJson<CustomerList>().customers;

                if (currentCustomers.Count == 0)
                {
                    break;
                }

                currentPage++;
            }

            _batchStateRepository.UpdateShopifyCustomersPullEnd(startOfPullRun);
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
                = _orderRepository.RetrieveShopifyCustomer(customer.id);

            if (existingCustomer == null)
            {
                var newCustomer = new UsrShopifyCustomer();
                newCustomer.ShopifyCustomerId = customer.id;
                newCustomer.ShopifyJson = customer.SerializeToJson();
                newCustomer.ShopifyPrimaryEmail = customer.email;
                newCustomer.DateCreated = DateTime.UtcNow;
                newCustomer.LastUpdated = DateTime.UtcNow;
                _orderRepository.InsertShopifyCustomer(newCustomer);
            }
            else
            {
                existingCustomer.ShopifyJson = customer.SerializeToJson();
                existingCustomer.ShopifyPrimaryEmail = customer.email;
                existingCustomer.LastUpdated = DateTime.UtcNow;
                _orderRepository.SaveChanges();
            }
        }        
    }
}

