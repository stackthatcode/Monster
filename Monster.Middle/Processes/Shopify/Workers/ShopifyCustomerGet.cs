using System;
using System.Collections.Generic;
using Monster.Middle.Persist.Instance;
using Monster.Middle.Processes.Shopify.Persist;
using Monster.Middle.Processes.Sync.Model.Status;
using Monster.Middle.Processes.Sync.Persist;
using Push.Foundation.Utilities.Json;
using Push.Shopify.Api;
using Push.Shopify.Api.Customer;

namespace Monster.Middle.Processes.Shopify.Workers
{
    public class ShopifyCustomerGet
    {
        private readonly CustomerApi _customerApi;
        private readonly ShopifyOrderRepository _orderRepository;
        private readonly ShopifyBatchRepository _batchRepository;
        private readonly PreferencesRepository _preferencesRepository;

        
        public ShopifyCustomerGet(
                CustomerApi customerApi,
                ShopifyOrderRepository orderRepository,
                ShopifyBatchRepository batchRepository,
                PreferencesRepository preferencesRepository)
        {
            _customerApi = customerApi;
            _orderRepository = orderRepository;
            _batchRepository = batchRepository;
            _preferencesRepository = preferencesRepository;
        }


        public void RunAutomatic()
        {
            var preferences = _preferencesRepository.RetrievePreferences();
            preferences.AssertStartingOrderIsValid();

            var batchState = _batchRepository.Retrieve();

            if (batchState.ShopifyCustomersGetEnd == null)
            {
                var firstFilter = new SearchFilter();
                firstFilter.Page = 1;
                firstFilter.UpdatedAtMinUtc = preferences.ShopifyOrderCreatedAtUtc.Value;
                    
                Run(firstFilter);
            }
            else
            {
                var firstFilter = new SearchFilter();
                firstFilter.Page = 1;
                firstFilter.UpdatedAtMinUtc = batchState.ShopifyCustomersGetEnd.Value;

                Run(firstFilter);
            }
        }

        private void Run(SearchFilter firstFilter)
        {
            var startOfRun = DateTime.UtcNow;
            var firstJson = _customerApi.Retrieve(firstFilter);
            var firstCustomers = firstJson.DeserializeFromJson<CustomerList>().customers;

            UpsertCustomers(firstCustomers);
            var currentPage = 2;

            while (true)
            {
                var currentFilter = firstFilter.Clone();
                currentFilter.Page = currentPage;
                var currentJson = _customerApi.Retrieve(currentFilter);
                var currentCustomers = currentJson.DeserializeFromJson<CustomerList>().customers;

                if (currentCustomers.Count == 0)
                {
                    break;
                }

                UpsertCustomers(currentCustomers);
                currentPage++;
            }

            // Compute the Batch State Pull End
            //
            var batchEnd = (startOfRun).SubtractFudgeFactor();
            _batchRepository.UpdateCustomersGetEnd(batchEnd);
        }

        public ShopifyCustomer Run(long shopifyCustomerId)
        {
           var customerJson = _customerApi.Retrieve(shopifyCustomerId);
           var customer = customerJson.DeserializeFromJson<CustomerParent>().customer;

           return UpsertCustomer(customer);
        }

        private void UpsertCustomers(List<Customer> customers)
        {
            foreach (var customer in customers)
            {
                UpsertCustomer(customer);
            }
        }

        public ShopifyCustomer UpsertCustomer(Customer customer)
        {
            var existingCustomer 
                = _orderRepository.RetrieveCustomer(customer.id);

            if (existingCustomer == null)
            {
                var newCustomer = new ShopifyCustomer();
                newCustomer.ShopifyCustomerId = customer.id;
                newCustomer.ShopifyJson = customer.SerializeToJson();
                newCustomer.ShopifyPrimaryEmail = customer.email;
                newCustomer.DateCreated = DateTime.UtcNow;
                newCustomer.LastUpdated = DateTime.UtcNow;
                newCustomer.NeedsCustomerPut = true;

                _orderRepository.InsertCustomer(newCustomer);
                return newCustomer;
            }
            else
            {
                existingCustomer.ShopifyJson = customer.SerializeToJson();
                existingCustomer.ShopifyPrimaryEmail = customer.email;
                existingCustomer.NeedsCustomerPut = true;
                existingCustomer.LastUpdated = DateTime.UtcNow;

                _orderRepository.SaveChanges();
                return existingCustomer;
            }
        }        
    }
}

