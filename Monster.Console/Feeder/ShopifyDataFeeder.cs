using System;
using System.Collections.Generic;
using System.Linq;
using Monster.Middle.Persist.Instance;
using Monster.Middle.Persist.Master;
using Push.Foundation.Utilities.Json;
using Push.Foundation.Utilities.Logging;
using Push.Shopify.Api;
using Push.Shopify.Api.Customer;
using Push.Shopify.Api.Order;

namespace Monster.ConsoleApp.Feeder
{
    public class ShopifyDataFeeder
    {
        private readonly InstanceContext _connection;
        private readonly OrderApi _orderApi;
        private readonly CustomerApi _customerApi;
        private readonly IPushLogger _logger;

        public ShopifyDataFeeder(
                InstanceContext connection, 
                OrderApi orderApi, 
                CustomerApi customerApi, 
                IPushLogger logger)
        {
            _connection = connection;
            _orderApi = orderApi;
            _customerApi = customerApi;
            _logger = logger;
        }

        public void Run()
        {
            var instanceId = Guid.Parse("51AA413D-E679-4F38-BA47-68129B3F9212");
            _connection.Initialize(instanceId);
            var testCustomerJson 
                = System.IO.File.ReadAllText("Feeder/TestCustomers.json");

            var testCustomers = testCustomerJson.DeserializeFromJson<List<TestCustomerDto>>();
            var count = 0;
            foreach (var customer in testCustomers)
            {
                var standardizedEmail
                    = customer.Email.Substring(0, customer.Email.IndexOf("@") + 1)
                      + "logicautomated.com";

                var payload = new OrderPayload
                {
                    CustomerEmail = standardizedEmail,
                    CustomerFirstName = customer.FirstName,
                    CustomerLastName = customer.LastName,
                    LineItemVariantId = 12489243918434, // MED123
                    UnitPrice = 50,
                    Quantity = 2,
                };
                PushCustomerOrderTransaction(payload);
                _logger.Info($"Created Customer+Order+Transaction - {++count}");
            }
        }

        public void PushCustomerOrderTransaction(OrderPayload payload)
        { 
            var customerId = EnsureCustomerExists(payload);

            var order = _orderApi
                .Insert(payload.ToOrder(customerId))
                .DeserializeFromJson<OrderParent>()
                .order;

            //var transaction = _orderApi
            //    .InsertTransaction(order.id, payload.ToOrderTransaction())
            //    .DeserializeFromJson<TransactionParent>()
            //    .transaction;
        }

        public long EnsureCustomerExists(OrderPayload payload)
        {
            var existing = _customerApi.Search($"email:{payload.CustomerEmail}");
            var results = existing.DeserializeFromJson<CustomerList>().customers;
            if (results.Count == 0)
            {
                var newCustomerJson = _customerApi.Create(payload.CustomerJson);
                return newCustomerJson.DeserializeFromJson<CustomerParent>().customer.id;
            }
            else
            {
                return results.First().id;
            }
        }
    }
}

