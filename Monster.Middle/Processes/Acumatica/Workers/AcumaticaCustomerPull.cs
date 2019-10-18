﻿using System;
using System.Collections.Generic;
using Monster.Acumatica.Api;
using Monster.Acumatica.Api.Customer;
using Monster.Middle.Misc.External;
using Monster.Middle.Persist.Instance;
using Monster.Middle.Processes.Acumatica.Persist;
using Monster.Middle.Processes.Sync.Model.Orders;
using Monster.Middle.Processes.Sync.Persist;
using Monster.Middle.Utility;
using Push.Foundation.Utilities.Json;
using Push.Foundation.Utilities.Logging;

namespace Monster.Middle.Processes.Acumatica.Workers
{
    public class AcumaticaCustomerPull
    {
        private readonly CustomerClient _customerClient;
        private readonly AcumaticaOrderRepository _orderRepository;
        private readonly AcumaticaBatchRepository _batchStateRepository;
        private readonly AcumaticaTimeZoneService _instanceTimeZoneService;
        private readonly ExternalServiceRepository _connectionRepository;
        private readonly PreferencesRepository _preferencesRepository;
        private readonly IPushLogger _logger;

        public const int InitialBatchStateFudgeMin = -15;


        public AcumaticaCustomerPull(
                CustomerClient customerClient,
                AcumaticaOrderRepository orderRepository,
                AcumaticaBatchRepository batchStateRepository,
                AcumaticaTimeZoneService instanceTimeZoneService,
                ExternalServiceRepository connectionRepository,
                PreferencesRepository preferencesRepository,
                IPushLogger logger)
        {
            _customerClient = customerClient;
            _orderRepository = orderRepository;
            _batchStateRepository = batchStateRepository;
            _instanceTimeZoneService = instanceTimeZoneService;
            _connectionRepository = connectionRepository;
            _preferencesRepository = preferencesRepository;
            _logger = logger;
        }


        public void RunAutomatic()
        {
            var startOfRun = DateTime.UtcNow;
            var batchState = _batchStateRepository.Retrieve();
            
            if (batchState.AcumaticaCustomersPullEnd.HasValue)
            {
                RunUpdated();
            }
            else
            {
                RunAll();
            }

            // Set the Batch State Pull End marker
            var pullEnd = (startOfRun).AddAcumaticaBatchFudge();
            _batchStateRepository.UpdateCustomersPullEnd(pullEnd);
        }

        private void RunAll()
        {
            var preferences = _preferencesRepository.RetrievePreferences();
            var customerUpdateMin = preferences.ShopifyOrderDateStart;
            
            var json = _customerClient.RetrieveCustomers(customerUpdateMin);
            var customers = json.DeserializeFromJson<List<Customer>>();

            UpsertCustomersToPersist(customers);
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
            var updateMin = _instanceTimeZoneService.ToAcumaticaTimeZone(updateMinUtc.Value);

            var json = _customerClient.RetrieveCustomers(updateMin);
            var customers = json.DeserializeFromJson<List<Customer>>();
            UpsertCustomersToPersist(customers);
        }
        
        public void UpsertCustomersToPersist(List<Customer> customers)
        {
            foreach (var customer in customers)
            {
                UpsertCustomerToPersist(customer);
            }
        }

        public AcumaticaCustomer UpsertCustomerToPersist(Customer customer)
        {
            var existingData
                = _orderRepository.RetrieveCustomer(customer.CustomerID.value);

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
        
        public long RunAndUpsertCustomerIfNotExists(string acumaticaCustomerId)
        {
            var existingCustomer
                    = _orderRepository.RetrieveCustomer(acumaticaCustomerId);

            if (existingCustomer == null)
            {
                var customerJson = _customerClient.RetrieveCustomer(acumaticaCustomerId);
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

