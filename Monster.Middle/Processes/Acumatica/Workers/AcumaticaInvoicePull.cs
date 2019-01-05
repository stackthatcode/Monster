using System;
using System.Collections.Generic;
using Monster.Acumatica.Api;
using Monster.Acumatica.Api.SalesOrder;
using Monster.Acumatica.Api.Shipment;
using Monster.Middle.Persist.Multitenant;
using Monster.Middle.Processes.Acumatica.Persist;
using Monster.Middle.Services;
using Push.Foundation.Utilities.Json;
using Push.Foundation.Utilities.Logging;

namespace Monster.Middle.Processes.Acumatica.Workers
{
    public class AcumaticaInvoicePull
    {
        private readonly SalesOrderClient _salesOrderClient;
        private readonly TenantRepository _tenantRepository;
        private readonly AcumaticaOrderRepository _orderRepository;
        private readonly AcumaticaBatchRepository _batchStateRepository;
        private readonly TimeZoneService _timeZoneService;
        private readonly IPushLogger _logger;

        public const int InitialBatchStateFudgeMin = -15;


        public AcumaticaInvoicePull(

                AcumaticaOrderRepository orderRepository,
                AcumaticaBatchRepository batchStateRepository,
                TimeZoneService timeZoneService,
                SalesOrderClient salesOrderClient,
                TenantRepository tenantRepository,
                IPushLogger logger)
        {
            _orderRepository = orderRepository;
            _batchStateRepository = batchStateRepository;
            _timeZoneService = timeZoneService;
            _salesOrderClient = salesOrderClient;
            _tenantRepository = tenantRepository;
            _logger = logger;
        }
        

        public void RunAutomatic()
        {
            // Pull based on date range
            var batchState = _batchStateRepository.Retrieve();
            if (batchState.AcumaticaInvoicesPullEnd.HasValue)
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
            var updateMin = preferences.ShopifyOrderDateStart;

            //var json = _salesOrderClient.RetrieveSalesOrderInvoices(updateMin);

            // *** WRITE TO PERSISTENCE
            //var invoices = json.DeserializeFromJson<List<SalesInvoice>>();
            //UpsertOrdersToPersist(orders);

            // Set the Batch State Pull End marker
            //var maxOrderDate = _orderRepository.RetrieveInvoiceMaxUpdatedDate();

            //var batchStateEnd
            //    = maxOrderDate
            //        ?? DateTime.UtcNow.AddMinutes(InitialBatchStateFudgeMin);

            //_batchStateRepository.UpdateInvoicesPullEnd(batchStateEnd);
        }

        private void RunUpdated()
        {
            var batchState = _batchStateRepository.Retrieve();
            
            var updateMinUtc = batchState.AcumaticaInvoicesPullEnd;
            var updateMin = _timeZoneService.ToAcumaticaTimeZone(updateMinUtc.Value);

            var pullRunStartTime = DateTime.UtcNow;
            //var json = _salesOrderClient.RetrieveSalesOrderInvoices(updateMin);
            
            //var orders = json.DeserializeFromJson<List<SalesOrder>>();
            //UpsertOrdersToPersist(orders);

            _batchStateRepository.UpdateCustomersPullEnd(pullRunStartTime);
        }
        
        public void UpsertInvoicesToPersist(List<SalesOrder> orders)
        {
            foreach (var order in orders)
            {
                //UpsertOrderToPersist(order);
            }
        }

        public UsrAcumaticaShipment 
                    UpsertInvoiceToPersist(Shipment shipment)
        {
            var shipmentNbr = shipment.ShipmentNbr.value;

            var existingData
                = _orderRepository
                    .RetrieveShipment(shipmentNbr);

            if (existingData == null)
            {
                var newData = new UsrAcumaticaShipment();
                newData.AcumaticaJson = shipment.SerializeToJson();
                newData.AcumaticaShipmentNbr = shipment.ShipmentNbr.value;
                newData.AcumaticaStatus = shipment.Status.value;
                newData.DateCreated = DateTime.UtcNow;
                newData.LastUpdated = DateTime.UtcNow;
                _orderRepository.InsertShipment(newData);

                return newData;
            }
            else
            {
                existingData.AcumaticaJson = shipment.SerializeToJson();
                existingData.AcumaticaStatus = shipment.Status.value;
                existingData.LastUpdated = DateTime.UtcNow;
                _orderRepository.SaveChanges();
                                
                return existingData;
            }
        }
    }
}

