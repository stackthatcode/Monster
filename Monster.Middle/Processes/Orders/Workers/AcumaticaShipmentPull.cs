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
    public class AcumaticaShipmentPull
    {
        private readonly CustomerClient _customerClient;
        private readonly ShipmentClient _shipmentClient;
        private readonly OrderRepository _orderRepository;
        private readonly TenantRepository _tenantRepository;
        private readonly BatchStateRepository _batchStateRepository;
        private readonly IPushLogger _logger;

        public const int InitialBatchStateFudgeMin = -15;


        public AcumaticaShipmentPull(
                CustomerClient customerClient, 
                OrderRepository orderRepository,
                BatchStateRepository batchStateRepository,
                ShipmentClient shipmentClient,
                TenantRepository tenantRepository,
                IPushLogger logger)
        {
            _customerClient = customerClient;
            _orderRepository = orderRepository;
            _batchStateRepository = batchStateRepository;
            _shipmentClient = shipmentClient;
            _tenantRepository = tenantRepository;
            _logger = logger;
        }


        public void RunAll()
        {
            var preferences = _tenantRepository.RetrievePreferences();
            var orderUpdateMin = preferences.DataPullStart;

            var json = _shipmentClient.RetrieveShipments(orderUpdateMin);

            //var orders = json.DeserializeFromJson<List<Shipment>>();

            //UpsertOrdersToPersist(orders);

            //// Set the Batch State Pull End marker
            //var maxOrderDate =
            //    _orderRepository.RetrieveShopifyOrderMaxUpdatedDate();

            //var batchStateEnd 
            //    = maxOrderDate
            //        ?? DateTime.UtcNow.AddMinutes(InitialBatchStateFudgeMin);

            //_batchStateRepository
            //    .UpdateAcumaticaOrdersPullEnd(batchStateEnd);
        }
        
        public void RunUpdated()
        {
            //var batchState = _batchStateRepository.RetrieveBatchState();
            //if (!batchState.AcumaticaOrdersPullEnd.HasValue)
            //{
            //    throw new Exception(
            //        "AcumaticaOrdersPullEnd is null - execute RunAll() first");
            //}

            //var updateMin = batchState.AcumaticaOrdersPullEnd;
            //var pullRunStartTime = DateTime.UtcNow;

            //var json = _shipmentClient.RetrieveSalesOrders(updateMin);
            //var orders = json.DeserializeFromJson<List<SalesOrder>>();

            //UpsertOrdersToPersist(orders);

            //_batchStateRepository.UpdateAcumaticaCustomersPullEnd(pullRunStartTime);
        }

        public void UpsertOrdersToPersist(List<SalesOrder> orders)
        {
            foreach (var order in orders)
            {
                UpsertOrderToPersist(order);
            }
        }

        public UsrAcumaticaSalesOrder UpsertOrderToPersist(SalesOrder order)
        {
            var orderNbr = order.OrderNbr.value;

            var existingData
                = _orderRepository
                    .RetrieveAcumaticaSalesOrder(orderNbr);

            if (existingData == null)
            {
                // Locate Acumatica Customer..
                var customerId = order.CustomerID.value;

                var customerMonsterId
                    = LocateOrPullAndUpsertCustomer(customerId);

                var newData = new UsrAcumaticaSalesOrder();
                newData.AcumaticaSalesOrderId = orderNbr;
                newData.AcumaticaJson = order.SerializeToJson();
                newData.CustomerMonsterId = customerMonsterId;

                newData.DateCreated = DateTime.UtcNow;
                newData.LastUpdated = DateTime.UtcNow;

                _orderRepository.InsertAcumaticaSalesOrder(newData);

                return newData;
            }
            else
            {
                existingData.AcumaticaJson = order.SerializeToJson();
                existingData.LastUpdated = DateTime.UtcNow;

                _orderRepository.SaveChanges();

                return existingData;
            }
        }

        private long LocateOrPullAndUpsertCustomer(string acumaticaCustomerId)
        {
            var existingCustomer
                = _orderRepository.RetrieveAcumaticaCustomer(acumaticaCustomerId);

            if (existingCustomer == null)
            {
                var customerJson 
                    = _customerClient.RetrieveCustomer(acumaticaCustomerId);
                var customer = customerJson.DeserializeFromJson<Customer>();

                var newData = customer.ToMonsterRecord();                
                _orderRepository.InsertAcumaticaCustomer(newData);

                return newData.Id;
            }
            else
            {
                return existingCustomer.Id;
            }
        }
    }
}

