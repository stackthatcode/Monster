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
        private readonly CustomerClient _customerClient;
        private readonly SalesOrderClient _salesOrderClient;
        private readonly AcumaticaBatchRepository _batchStateRepository;
        private readonly AcumaticaOrderRepository _orderRepository;
        private readonly AcumaticaCustomerPull _acumaticaCustomerPull;
        private readonly TimeZoneService _timeZoneService;
        private readonly TenantRepository _tenantRepository;
        private readonly IPushLogger _logger;

        public const int InitialBatchStateFudgeMin = -15;


        public AcumaticaOrderPull(
                CustomerClient customerClient, 
                AcumaticaOrderRepository orderRepository,
                AcumaticaCustomerPull acumaticaCustomerPull,
                TimeZoneService timeZoneService,
                AcumaticaBatchRepository batchStateRepository,
                
                SalesOrderClient salesOrderClient,
                TenantRepository tenantRepository,
                IPushLogger logger)
        {
            _customerClient = customerClient;
            _orderRepository = orderRepository;
            _acumaticaCustomerPull = acumaticaCustomerPull;
            _timeZoneService = timeZoneService;
            _batchStateRepository = batchStateRepository;
            _salesOrderClient = salesOrderClient;
            _tenantRepository = tenantRepository;
            
            _logger = logger;
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
            var preferences = _tenantRepository.RetrievePreferences();
            var orderUpdateMin = preferences.ShopifyDataPullStart;

            var json = _salesOrderClient.RetrieveSalesOrders(orderUpdateMin);
            var orders = json.DeserializeFromJson<List<SalesOrder>>();

            UpsertOrdersToPersist(orders);

            // Set the Batch State Pull End marker
            var maxOrderDate =
                _orderRepository.RetrieveOrderMaxUpdatedDate();

            var batchStateEnd 
                = maxOrderDate
                    ?? DateTime.UtcNow.AddMinutes(InitialBatchStateFudgeMin);

            _batchStateRepository.UpdateOrdersPullEnd(batchStateEnd);
        }

        private void RunUpdated()
        {
            var batchState = _batchStateRepository.Retrieve();
            var updateMinUtc = batchState.AcumaticaOrdersPullEnd;
            var updateMin = _timeZoneService.ToAcumaticaTimeZone(updateMinUtc.Value);

            var pullRunStartTime = DateTime.UtcNow;

            var json = _salesOrderClient.RetrieveSalesOrders(updateMin);
            var orders = json.DeserializeFromJson<List<SalesOrder>>();

            UpsertOrdersToPersist(orders);

            _batchStateRepository.UpdateOrdersPullEnd(pullRunStartTime);
        }

        
        public void UpsertOrdersToPersist(List<SalesOrder> orders)
        {
            foreach (var order in orders)
            {
                UpsertOrderToPersist(order);
                PullAndStubNewShipments(order.OrderNbr.value);
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
                    = _acumaticaCustomerPull.RunAndUpsertCustomer(customerId);

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
        
        public void PullAndStubNewShipments(string orderNbr)
        {
            var json = _salesOrderClient.RetrieveSalesOrderShipments(orderNbr);
            var orderRecord = _orderRepository.RetrieveSalesOrder(orderNbr);
            var order = json.DeserializeFromJson<SalesOrder>();

            // First, update the Sales Order record
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

            _orderRepository
                .ImprintSoShipmentInvoices(orderRecord.Id, translation);
        }
    }
}

