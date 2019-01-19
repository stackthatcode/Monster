using System;
using System.Collections.Generic;
using Monster.Acumatica.Api;
using Monster.Acumatica.Api.SalesOrder;
using Monster.Middle.Persist.Multitenant;
using Monster.Middle.Processes.Acumatica.Persist;
using Monster.Middle.Services;
using Push.Foundation.Utilities.Json;
using Push.Foundation.Utilities.Logging;


namespace Monster.Middle.Processes.Acumatica.Workers
{
    public class AcumaticaOrderPull
    {
        private readonly SalesOrderClient _salesOrderClient;
        private readonly AcumaticaBatchRepository _batchStateRepository;
        private readonly AcumaticaOrderRepository _orderRepository;
        private readonly AcumaticaCustomerPull _customerPull;
        private readonly InstanceTimeZoneService _timeZoneService;
        private readonly PreferencesRepository _preferencesRepository;


        public AcumaticaOrderPull(
                AcumaticaOrderRepository orderRepository,
                AcumaticaCustomerPull customerPull,
                InstanceTimeZoneService timeZoneService,
                AcumaticaBatchRepository batchStateRepository,
                SalesOrderClient salesOrderClient,
                PreferencesRepository preferencesRepository)
        {
            _orderRepository = orderRepository;
            _customerPull = customerPull;
            _timeZoneService = timeZoneService;
            _batchStateRepository = batchStateRepository;
            _salesOrderClient = salesOrderClient;
            _preferencesRepository = preferencesRepository;
        }


        public void RunAutomatic()
        {
            var batchState = _batchStateRepository.Retrieve();
            if (batchState.AcumaticaOrdersPullEnd.HasValue)
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
            var startOfRun = DateTime.UtcNow;

            var preferences = _preferencesRepository.RetrievePreferences();
            var orderStartDate = preferences.ShopifyOrderDateStart.Value;
            var orderUpdateMin = _timeZoneService.ToAcumaticaTimeZone(orderStartDate);
            
            var json = _salesOrderClient.RetrieveSalesOrders(orderUpdateMin);
            var orders = json.DeserializeFromJson<List<SalesOrder>>();

            UpsertOrdersToPersist(orders);

            // Set the Batch State Pull End marker
            var maxOrderDate = _orderRepository.RetrieveOrderMaxUpdatedDate();
            var pullEnd = (maxOrderDate ?? startOfRun).AddAcumaticaBatchFudge();
                
            _batchStateRepository.UpdateOrdersPullEnd(pullEnd);
        }

        private void RunUpdated()
        {
            var startOfRun = DateTime.UtcNow;

            var batchState = _batchStateRepository.Retrieve();
            var updateMinUtc = batchState.AcumaticaOrdersPullEnd;
            var updateMin = _timeZoneService.ToAcumaticaTimeZone(updateMinUtc.Value);

            var json = _salesOrderClient.RetrieveSalesOrders(updateMin);
            var orders = json.DeserializeFromJson<List<SalesOrder>>();

            UpsertOrdersToPersist(orders);

            _batchStateRepository.UpdateOrdersPullEnd(startOfRun);
        }

        
        public void UpsertOrdersToPersist(List<SalesOrder> orders)
        {
            foreach (var order in orders)
            {
                UpsertOrderToPersist(order);
                PullAndUpsertShipmentInvoiceRefs(order.OrderNbr.value);
            }
        }

        public UsrAcumaticaSalesOrder UpsertOrderToPersist(SalesOrder order)
        {
            var orderNbr = order.OrderNbr.value;
            var existingData = _orderRepository.RetrieveSalesOrder(orderNbr);

            if (existingData == null)
            {
                // Locate Acumatica Customer..
                var customerId = order.CustomerID.value;
                var customerMonsterId
                    = _customerPull.RunAndUpsertCustomerIfNotExists(customerId);

                var newData = new UsrAcumaticaSalesOrder();
                newData.AcumaticaOrderNbr = orderNbr;
                newData.DetailsJson = order.SerializeToJson();
                newData.ShipmentsJson = null;
                newData.AcumaticaStatus = order.Status.value;
                newData.CustomerMonsterId = customerMonsterId;
                newData.DateCreated = DateTime.UtcNow;
                newData.LastUpdated = DateTime.UtcNow;

                _orderRepository.InsertSalesOrder(newData);
                return newData;
            }
            else
            {
                existingData.DetailsJson = order.SerializeToJson();
                existingData.AcumaticaStatus = order.Status.value;
                existingData.LastUpdated = DateTime.UtcNow;
                
                _orderRepository.SaveChanges();
                return existingData;
            }
        }
        
        public void PullAndUpsertShipmentInvoiceRefs(string orderNbr)
        {
            var json = _salesOrderClient.RetrieveSalesOrderShipments(orderNbr);
            var orderRecord = _orderRepository.RetrieveSalesOrder(orderNbr);
            var order = json.DeserializeFromJson<SalesOrder>();

            orderRecord.ShipmentsJson = json;
            orderRecord.LastUpdated = DateTime.UtcNow;

            using (var transaction = _orderRepository.BeginTransaction())
            {
                UpsertShipmentInvoiceStubs(orderRecord, order.Shipments);
                transaction.Commit();
            }
        }        
        
        public void UpsertShipmentInvoiceStubs(
                    UsrAcumaticaSalesOrder orderRecord, 
                    List<SalesOrderShipment> shipments)
        {
            var translation = new List<UsrAcumaticaSoShipmentInvoice>();

            foreach (var shipment in shipments)
            {
                var record = new UsrAcumaticaSoShipmentInvoice();
                record.AcumaticaShipmentNbr = shipment.ShipmentNbr.value;
                record.AcumaticaInvoiceNbr = shipment.InvoiceNbr.value;

                translation.Add(record);
            }

            _orderRepository.ImprintSoShipmentInvoices(orderRecord.Id, translation);
        }
    }
}

