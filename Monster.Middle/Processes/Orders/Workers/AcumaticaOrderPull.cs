using System;
using System.Collections.Generic;
using Monster.Acumatica.Api;
using Monster.Acumatica.Api.Customer;
using Monster.Acumatica.Api.SalesOrder;
using Monster.Middle.Persist.Multitenant;
using Push.Foundation.Utilities.Json;
using Push.Foundation.Utilities.Logging;


namespace Monster.Middle.Processes.Orders.Workers
{
    public class AcumaticaOrderPull
    {
        private readonly CustomerClient _customerClient;
        private readonly SalesOrderClient _salesOrderClient;
        private readonly OrderRepository _orderRepository;
        private readonly TenantRepository _tenantRepository;
        private readonly BatchStateRepository _batchStateRepository;
        private readonly IPushLogger _logger;

        public const int InitialBatchStateFudgeMin = -15;


        public AcumaticaOrderPull(
                CustomerClient customerClient, 
                OrderRepository orderRepository,
                BatchStateRepository batchStateRepository,
                SalesOrderClient salesOrderClient,
                TenantRepository tenantRepository,
                IPushLogger logger)
        {
            _customerClient = customerClient;
            _orderRepository = orderRepository;
            _batchStateRepository = batchStateRepository;
            _salesOrderClient = salesOrderClient;
            _tenantRepository = tenantRepository;
            _logger = logger;
        }


        public void RunAll()
        {
            var preferences = _tenantRepository.RetrievePreferences();
            var orderUpdateMin = preferences.DataPullStart;

            var json = _salesOrderClient.RetrieveSalesOrders(orderUpdateMin);
            var orders = json.DeserializeFromJson<List<SalesOrder>>();

            UpsertOrdersToPersist(orders);

            // Set the Batch State Pull End marker
            var maxOrderDate =
                _orderRepository.RetrieveShopifyOrderMaxUpdatedDate();

            var batchStateEnd 
                = maxOrderDate
                    ?? DateTime.UtcNow.AddMinutes(InitialBatchStateFudgeMin);

            _batchStateRepository
                .UpdateAcumaticaCustomersPullEnd(batchStateEnd);
        }
        
        public void RunUpdated()
        {
            var batchState = _batchStateRepository.RetrieveBatchState();
            if (!batchState.AcumaticaOrdersPullEnd.HasValue)
            {
                throw new Exception(
                    "AcumaticaOrdersPullEnd is null - execute RunAll() first");
            }

            var updateMin = batchState.AcumaticaOrdersPullEnd;
            var pullRunStartTime = DateTime.UtcNow;

            var json = _salesOrderClient.RetrieveSalesOrders(updateMin);
            var orders = json.DeserializeFromJson<List<SalesOrder>>();

            UpsertOrdersToPersist(orders);

            _batchStateRepository.UpdateAcumaticaCustomersPullEnd(pullRunStartTime);
        }

        public void UpsertOrdersToPersist(List<SalesOrder> orders)
        {
            foreach (var order in orders)
            {
                //var existingData
                //    = _orderRepository
                //        .RetrieveAcumaticaSalesOrder(order.SalesOrderID.value);

                //if (existingData == null)
                //{
                //    var newData = new UsrAcumaticaCustomer()
                //    {
                //        AcumaticaCustomerId = customer.CustomerID.value,
                //        AcumaticaJson = customer.SerializeToJson(),
                //        DateCreated = DateTime.UtcNow,
                //        LastUpdated = DateTime.UtcNow,
                //    };

                //    _orderRepository.InsertAcumaticaCustomer(newData);
                //}
                //else
                //{
                //    existingData.AcumaticaJson = customer.SerializeToJson();
                //    existingData.LastUpdated = DateTime.UtcNow;

                //    _orderRepository.SaveChanges();
                //}
            }
        }
    }
}

