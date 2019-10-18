using System;
using System.Collections.Generic;
using Monster.Acumatica.Api;
using Monster.Acumatica.Api.Common;
using Monster.Acumatica.Api.SalesOrder;
using Monster.Acumatica.Config;
using Monster.Middle.Persist.Instance;
using Monster.Middle.Processes.Acumatica.Persist;
using Monster.Middle.Processes.Sync.Persist;
using Monster.Middle.Utility;
using Push.Foundation.Utilities.Json;


namespace Monster.Middle.Processes.Acumatica.Workers
{
    public class AcumaticaOrderPull
    {
        private readonly SalesOrderClient _salesOrderClient;
        private readonly AcumaticaBatchRepository _batchStateRepository;
        private readonly AcumaticaOrderRepository _orderRepository;
        private readonly AcumaticaCustomerPull _customerPull;
        private readonly AcumaticaTimeZoneService _timeZoneService;
        private readonly PreferencesRepository _preferencesRepository;

        public int PageSize = 50;


        public AcumaticaOrderPull(
                AcumaticaOrderRepository orderRepository,
                AcumaticaCustomerPull customerPull,
                AcumaticaTimeZoneService timeZoneService,
                AcumaticaBatchRepository batchStateRepository,
                AcumaticaHttpConfig acumaticaHttpConfig,
                SalesOrderClient salesOrderClient,
                PreferencesRepository preferencesRepository)
        {
            _orderRepository = orderRepository;
            _customerPull = customerPull;
            _timeZoneService = timeZoneService;
            _batchStateRepository = batchStateRepository;
            _salesOrderClient = salesOrderClient;
            _preferencesRepository = preferencesRepository;

            PageSize = acumaticaHttpConfig.PageSize;
        }


        public void RunAutomatic()
        {
            var batchState = _batchStateRepository.Retrieve();
            if (batchState.AcumaticaOrdersPullEnd.HasValue)
            {
                var updateMinUtc = batchState.AcumaticaOrdersPullEnd;
                var updateMin = _timeZoneService.ToAcumaticaTimeZone(updateMinUtc.Value);

                RunWithPaging(updateMin);
            }
            else
            {
                var preferences = _preferencesRepository.RetrievePreferences();
                if (preferences.StartingShopifyOrderCreatedAtUtc == null)
                {
                    throw new Exception("StartingShopifyOrderCreatedAtUtc has not been set");
                }

                var orderStartDate = preferences.StartingShopifyOrderCreatedAtUtc.Value;
                var orderUpdateMin = _timeZoneService.ToAcumaticaTimeZone(orderStartDate);

                RunWithPaging(orderUpdateMin);
            }
        }

        private void RunWithPaging(DateTime lastModified)
        {
            var startOfRun = DateTime.UtcNow;
            var page = 1;

            while (true)
            {
                var json = _salesOrderClient.RetrieveSalesOrders(lastModified, page, PageSize);
                var orders = json.DeserializeFromJson<List<SalesOrder>>();

                UpsertOrdersToPersist(orders);

                if (orders.Count == 0)
                {
                    break;
                }
                page++;
            }

            // Set the Batch State Pull End marker
            //
            var batchStateEnd = (startOfRun).AddAcumaticaBatchFudge();
            _batchStateRepository.UpdateOrdersPullEnd(batchStateEnd);
        }

        public void RunAcumaticaOrderDetails(string orderId)
        {
            var salesOrder = _salesOrderClient.RetrieveSalesOrderDetails(orderId);
            UpsertOrderToPersist(salesOrder.DeserializeFromJson<SalesOrder>());
        }

        public void UpsertOrdersToPersist(List<SalesOrder> orders)
        {
            foreach (var order in orders)
            {
                if (order.OrderType.value != SalesOrderType.SO)
                {
                    continue;
                }

                UpsertOrderToPersist(order);
                PullAndUpsertShipmentInvoiceRefs(order.OrderNbr.value);
            }
        }

        public AcumaticaSalesOrder UpsertOrderToPersist(SalesOrder order)
        {
            var orderNbr = order.OrderNbr.value;
            var existingData = _orderRepository.RetrieveSalesOrder(orderNbr);

            if (existingData == null)
            {
                // Locate Acumatica Customer...
                //
                var customerId = order.CustomerID.value;
                var customerMonsterId = _customerPull.RunAndUpsertCustomerIfNotExists(customerId);

                var newData = new AcumaticaSalesOrder();
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
                AcumaticaSalesOrder orderRecord, List<SalesOrderShipment> shipments)
        {
            var translation = new List<AcumaticaSoShipmentInvoice>();

            foreach (var shipment in shipments)
            {
                var record = new AcumaticaSoShipmentInvoice();
                record.AcumaticaShipmentNbr = shipment.ShipmentNbr.value;
                record.AcumaticaInvoiceNbr = shipment.InvoiceNbr.value;

                translation.Add(record);
            }

            _orderRepository.ImprintSoShipmentInvoices(orderRecord.Id, translation);
        }
    }
}

