using System;
using System.Collections.Generic;
using System.Linq;
using Monster.Acumatica.Api;
using Monster.Acumatica.Api.SalesOrder;
using Monster.Acumatica.Api.Shipment;
using Monster.Acumatica.Config;
using Monster.Middle.Misc.Acumatica;
using Monster.Middle.Misc.Hangfire;
using Monster.Middle.Misc.Logging;
using Monster.Middle.Persist.Instance;
using Monster.Middle.Processes.Acumatica.Persist;
using Monster.Middle.Processes.Sync.Misc;
using Monster.Middle.Processes.Sync.Persist;
using Push.Foundation.Utilities.Helpers;
using Push.Foundation.Utilities.Json;


namespace Monster.Middle.Processes.Acumatica.Workers
{
    public class AcumaticaOrderGet
    {
        private readonly SalesOrderClient _salesOrderClient;
        private readonly InvoiceClient _invoiceClient;
        private readonly ShipmentClient _shipmentClient;
        private readonly AcumaticaBatchRepository _batchStateRepository;
        private readonly AcumaticaHttpConfig _acumaticaHttpConfig;
        private readonly AcumaticaOrderRepository _orderRepository;
        private readonly AcumaticaTimeZoneService _timeZoneService;
        private readonly SettingsRepository _settingsRepository;
        private readonly ExecutionLogService _executionLogService;
        private readonly JobMonitoringService _jobMonitoringService;


        public AcumaticaOrderGet(
                AcumaticaOrderRepository orderRepository,
                AcumaticaTimeZoneService timeZoneService,
                AcumaticaBatchRepository batchStateRepository,
                AcumaticaHttpConfig acumaticaHttpConfig,
                SettingsRepository settingsRepository,
                SalesOrderClient salesOrderClient,
                ShipmentClient shipmentClient, 
                InvoiceClient invoiceClient, 
                ExecutionLogService executionLogService,
                JobMonitoringService jobMonitoringService)
        {
            _orderRepository = orderRepository;
            _timeZoneService = timeZoneService;
            _batchStateRepository = batchStateRepository;
            _acumaticaHttpConfig = acumaticaHttpConfig;
            _salesOrderClient = salesOrderClient;
            _settingsRepository = settingsRepository;
            _shipmentClient = shipmentClient;
            _invoiceClient = invoiceClient;
            _executionLogService = executionLogService;
            _jobMonitoringService = jobMonitoringService;
        }

        public void Run(string orderId)
        {
            var json = _salesOrderClient
                .RetrieveSalesOrder(SalesOrderType.SO, orderId, Expand.Shipments_ShippingSettings);
            var salesOrder = json.DeserializeFromJson<SalesOrder>();
            UpsertOrderShippingDetails(new List<SalesOrder>() { salesOrder });
        }

        public void RunAutomatic()
        {
            var batchState = _batchStateRepository.Retrieve();
            var settings = _settingsRepository.RetrieveSettings();
            
            if (batchState.AcumaticaOrdersGetEnd.HasValue)
            {
                var updateMinUtc = batchState.AcumaticaOrdersGetEnd.Value;
                RunWithPaging(updateMinUtc);
            }
            else
            {
                var orderStartDateUtc = settings.ShopifyOrderCreatedAtUtc.Value;
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
                if (_jobMonitoringService.DetectCurrentJobInterrupt())
                {
                    return;
                }

                var orders = 
                    _salesOrderClient.RetrieveUpdatedSalesOrders(
                        lastModifiedMin, page, pageSize, expand:Expand.Shipments_Totals);

                if (orders.Count == 0)
                {
                    break;
                }

                UpsertOrderShippingDetails(orders);
                page++;
            }

            // Set the Batch State Pull End marker
            //
            var batchStateEnd = (startOfRun).AddAcumaticaBatchFudge();
            _batchStateRepository.UpdateOrdersGetEnd(batchStateEnd);
        }

        public void UpsertOrderShippingDetails(List<SalesOrder> orders)
        {
            foreach (var order in orders)
            {
                if (order.OrderType.value != SalesOrderType.SO)
                {
                    continue;
                }

                var orderNbr = order.OrderNbr.value;
                var existingData = _orderRepository.RetrieveSalesOrder(orderNbr);

                if (existingData == null)
                {
                    // Skip Sales Orders that were not intentionally loaded into Acumatica
                    //
                    continue;
                }

                existingData.AcumaticaShipmentDetailsJson = order.SerializeToJson();
                existingData.AcumaticaFreight = (decimal)order.Totals.Freight.value;
                existingData.AcumaticaLineTotal = (decimal)order.Totals.LineTotalAmount.value;
                existingData.AcumaticaTaxTotal = (decimal)order.Totals.TaxTotal.value;
                existingData.AcumaticaOrderTotal = (decimal)order.OrderTotal.value;
                existingData.AcumaticaStatus = order.Status.value;
                existingData.AcumaticaIsTaxValid = order.IsTaxValid.value;
                existingData.LastUpdated = DateTime.UtcNow;

                _orderRepository.SaveChanges();

                UpsertSoShipments(existingData);

                PopulateSoShipments(existingData);
            }
        }


        public void UpsertSoShipments(AcumaticaSalesOrder salesOrderRecord)
        {
            var salesOrder = salesOrderRecord.ToSalesOrderObj();
            foreach (var shipment in salesOrder.Shipments)
            {
                var exists = 
                    _orderRepository.SoShipmentExists(
                        salesOrderRecord.ShopifyOrderMonsterId, 
                        shipment.ShipmentNbr.value, 
                        shipment.InvoiceNbr.value);

                if (exists)
                {
                    continue;
                }
                if (shipment.Status.value != SoShipmentStatus.Completed)
                {
                    continue;
                }

                var record = new AcumaticaSoShipment();
                record.AcumaticaSalesOrder = salesOrderRecord;
                record.AcumaticaShipmentNbr = shipment.ShipmentNbr.value;
                record.AcumaticaInvoiceNbr = shipment.InvoiceNbr.value;
                record.AcumaticaInvoiceType = shipment.InvoiceType.value;
                record.AcumaticaStatus = shipment.Status.value;
                record.AcumaticaShipmentJson = null;
                record.AcumaticaTrackingNbr = null;
                record.AcumaticaInvoiceAmount = null;
                record.AcumaticaInvoiceTax = null;
                record.NeedShipmentAndInvoiceGet = true;
                record.DateCreated = DateTime.UtcNow;
                record.LastUpdated = DateTime.UtcNow;

                _executionLogService.Log(LogBuilder.DetectedNewAcumaticaSoShipment(record));
                _orderRepository.InsertSoShipmentInvoice(record);
            }
        }

        public void PopulateSoShipments(AcumaticaSalesOrder salesOrderRecord)
        {
            var soShipments = 
                _orderRepository.RetrieveSoShipments(salesOrderRecord.ShopifyOrderMonsterId);

            foreach (var soShipment in soShipments)
            {
                if (soShipment.NeedShipmentAndInvoiceGet == false)
                {
                    continue;
                }

                if (soShipment.AcumaticaShipmentJson.IsNullOrEmpty() ||
                    soShipment.AcumaticaTrackingNbr.IsNullOrEmpty())
                {
                    var shipmentJson = _shipmentClient.RetrieveShipment(soShipment.AcumaticaShipmentNbr);
                    var shipment = shipmentJson.DeserializeFromJson<Shipment>();

                    var trackingNumber = shipment.Packages.FirstOrDefault()?.TrackingNbr.value;

                    soShipment.AcumaticaShipmentJson = shipmentJson;
                    soShipment.AcumaticaTrackingNbr = trackingNumber.IsNullOrEmptyAlt(BlankTrackingNumber());

                    _orderRepository.SaveChanges();
                }

                if (soShipment.AcumaticaInvoiceAmount == null || soShipment.AcumaticaInvoiceTax == null)
                {
                    var invoiceJson =
                        _invoiceClient.RetrieveInvoiceAndTaxes(
                                soShipment.AcumaticaInvoiceNbr, soShipment.AcumaticaInvoiceType);

                    var invoice = invoiceJson.DeserializeFromJson<Invoice>();

                    soShipment.AcumaticaInvoiceAmount = (decimal)invoice.Amount.value;
                    soShipment.AcumaticaInvoiceTax = (decimal) invoice.TaxTotal.value;
                    //TaxDetails.Sum(x => x.TaxAmount.value);

                    _orderRepository.SaveChanges();
                }

                soShipment.NeedShipmentAndInvoiceGet = false;
                _orderRepository.SaveChanges();
            }
        }

        public static string BlankTrackingNumber()
        {
            return $"(BLANK TRACKING #) - {Guid.NewGuid()}";
        }


    }
}

