using System;
using System.Collections.Generic;
using Monster.Acumatica.Api;
using Monster.Acumatica.Api.SalesOrder;
using Monster.Acumatica.Config;
using Monster.Middle.Persist.Instance;
using Monster.Middle.Processes.Acumatica.Persist;
using Monster.Middle.Processes.Sync.Model.Status;
using Monster.Middle.Processes.Sync.Persist;
using Monster.Middle.Utility;
using Push.Foundation.Utilities.Json;


namespace Monster.Middle.Processes.Acumatica.Workers
{
    public class AcumaticaOrderGet
    {
        private readonly SalesOrderClient _salesOrderClient;
        private readonly AcumaticaBatchRepository _batchStateRepository;
        private readonly AcumaticaHttpConfig _acumaticaHttpConfig;
        private readonly AcumaticaOrderRepository _orderRepository;
        private readonly AcumaticaCustomerGet _customerPull;
        private readonly AcumaticaTimeZoneService _timeZoneService;
        private readonly PreferencesRepository _preferencesRepository;


        public AcumaticaOrderGet(
                AcumaticaOrderRepository orderRepository,
                AcumaticaCustomerGet customerPull,
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
            _acumaticaHttpConfig = acumaticaHttpConfig;
            _salesOrderClient = salesOrderClient;
            _preferencesRepository = preferencesRepository;
        }


        public void RunAutomatic()
        {
            var batchState = _batchStateRepository.Retrieve();
            var preferences = _preferencesRepository.RetrievePreferences();
            preferences.AssertStartingOrderIsValid();

            if (batchState.AcumaticaOrdersGetEnd.HasValue)
            {
                var updateMinUtc = batchState.AcumaticaOrdersGetEnd.Value;
                RunWithPaging(updateMinUtc);
            }
            else
            {
                var orderStartDateUtc = preferences.ShopifyOrderCreatedAtUtc.Value;

                RunWithPaging(orderStartDateUtc);
            }
        }

        private void RunWithPaging(DateTime lastModifiedMinUtc)
        {
            var startOfRun = DateTime.UtcNow;
            var lastModifiedMin = _timeZoneService.ToAcumaticaTimeZone(lastModifiedMinUtc);

            var page = 1;
            var pageSize = _acumaticaHttpConfig.PageSize;

            while (true)
            {
                var json = _salesOrderClient.RetrieveSalesOrders(lastModifiedMin, page, pageSize);
                var orders = json.DeserializeFromJson<List<SalesOrder>>();

                if (orders.Count == 0)
                {
                    break;
                }

                UpsertOrdersToPersist(orders);
                page++;
            }

            // Set the Batch State Pull End marker
            //
            var batchStateEnd = (startOfRun).AddAcumaticaBatchFudge();
            _batchStateRepository.UpdateOrdersGetEnd(batchStateEnd);
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
            }
        }

        public AcumaticaSalesOrder UpsertOrderToPersist(SalesOrder order)
        {
            var orderNbr = order.OrderNbr.value;
            var existingData = _orderRepository.RetrieveSalesOrder(orderNbr);

            
            if (existingData != null)
            {
                existingData.DetailsJson = order.SerializeToJson();
                existingData.AcumaticaStatus = order.Status.value;
                existingData.LastUpdated = DateTime.UtcNow;
                _orderRepository.SaveChanges();

                PullAndUpsertShipmentInvoiceRefs(orderNbr);
                return existingData;
            }
            else
            {
                // *** TODO Disaster Recovery
                //
                return null;
            }
        }

        // *** SAVE - Specifically for instances where the Sales Order in Acumatica was loaded
        // ... from Monster, but Monster experienced data-loss i.e. this is disaster recovery
        //
        public void SalesOrderRecovery(SalesOrder order)
        {
            //var orderNbr = order.OrderNbr.value;

            //// Locate Acumatica Customer...
            ////
            //var customerId = order.CustomerID.value;

            //// *** Pending Disaster Recovery
            ////
            //var customerMonsterId = _customerPull.RunAndUpsertCustomerIfNotExists(customerId);

            //var newData = new AcumaticaSalesOrder();
            //newData.AcumaticaOrderNbr = orderNbr;
            //newData.DetailsJson = order.SerializeToJson();
            //newData.ShipmentsJson = null;
            //newData.AcumaticaStatus = order.Status.value;
            //newData.CustomerMonsterId = customerMonsterId;
            //newData.DateCreated = DateTime.UtcNow;
            //newData.LastUpdated = DateTime.UtcNow;

            //_orderRepository.InsertSalesOrder(newData);
            //return newData;
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

