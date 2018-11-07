using System;
using System.Collections.Generic;
using Monster.Acumatica.Api;
using Monster.Acumatica.Api.SalesOrder;
using Monster.Acumatica.Api.Shipment;
using Monster.Middle.Persist.Multitenant;
using Monster.Middle.Persist.Multitenant.Acumatica;
using Monster.Middle.Persist.Multitenant.Etc;
using Monster.Middle.Persist.Multitenant.Shopify;
using Push.Foundation.Utilities.Json;
using Push.Foundation.Utilities.Logging;


namespace Monster.Middle.Processes.Orders.Workers
{
    public class AcumaticaShipmentPull
    {
        private readonly ShipmentClient _shipmentClient;
        private readonly AcumaticaOrderRepository _orderRepository;
        private readonly TenantRepository _tenantRepository;
        private readonly BatchStateRepository _batchStateRepository;
        private readonly IPushLogger _logger;

        public const int InitialBatchStateFudgeMin = -15;


        public AcumaticaShipmentPull(
                AcumaticaOrderRepository orderRepository,
                BatchStateRepository batchStateRepository,
                ShipmentClient shipmentClient,
                TenantRepository tenantRepository,
                IPushLogger logger)
        {
            _orderRepository = orderRepository;
            _batchStateRepository = batchStateRepository;
            _shipmentClient = shipmentClient;
            _tenantRepository = tenantRepository;
            _logger = logger;
        }


        public void RunAll()
        {
            var preferences = _tenantRepository.RetrievePreferences();
            var shipmentUpdateMin = preferences.DataPullStart;

            var json = _shipmentClient.RetrieveShipments(shipmentUpdateMin);

            var shipments = json.DeserializeFromJson<List<Shipment>>();

            UpsertShipmentsToPersist(shipments);

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

        public void UpsertShipmentsToPersist(List<Shipment> shipments)
        {
            foreach (var shipment in shipments)
            {
                UpsertShipmentToPersist(shipment);
            }
        }

        public UsrAcumaticaShipment 
                    UpsertShipmentToPersist(Shipment shipment)
        {
            var shipmentNbr = shipment.ShipmentNbr.value;

            var existingData
                = _orderRepository
                    .RetrieveShipment(shipmentNbr);

            if (existingData == null)
            {
                // Locate Acumatica Customer..
                //var customerId = order.CustomerID.value;

                //var customerMonsterId
                //    = LocateOrPullAndUpsertCustomer(customerId);

                //var newData = new UsrAcumaticaSalesOrder();
                //newData.AcumaticaSalesOrderId = orderNbr;
                //newData.AcumaticaJson = order.SerializeToJson();
                //newData.CustomerMonsterId = customerMonsterId;

                //newData.DateCreated = DateTime.UtcNow;
                //newData.LastUpdated = DateTime.UtcNow;

                //_orderRepository.InsertAcumaticaSalesOrder(newData);

                //return newData;
                return null;
            }
            else
            {
                //existingData.AcumaticaJson = order.SerializeToJson();
                //existingData.LastUpdated = DateTime.UtcNow;

                _orderRepository.SaveChanges();

                return existingData;
            }
        }
    }
}

