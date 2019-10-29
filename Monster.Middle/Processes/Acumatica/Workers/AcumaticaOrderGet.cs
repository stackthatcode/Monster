using System;
using System.Collections.Generic;
using System.Linq;
using Monster.Acumatica.Api;
using Monster.Acumatica.Api.SalesOrder;
using Monster.Acumatica.Api.Shipment;
using Monster.Acumatica.Config;
using Monster.Middle.Persist.Instance;
using Monster.Middle.Processes.Acumatica.Persist;
using Monster.Middle.Processes.Sync.Model.Status;
using Monster.Middle.Processes.Sync.Persist;
using Monster.Middle.Utility;
using Push.Foundation.Utilities.Helpers;
using Push.Foundation.Utilities.Json;


namespace Monster.Middle.Processes.Acumatica.Workers
{
    public class AcumaticaOrderGet
    {
        private readonly SalesOrderClient _salesOrderClient;
        private readonly ShipmentClient _shipmentClient;
        private readonly AcumaticaBatchRepository _batchStateRepository;
        private readonly AcumaticaHttpConfig _acumaticaHttpConfig;
        private readonly AcumaticaOrderRepository _orderRepository;
        private readonly AcumaticaTimeZoneService _timeZoneService;
        private readonly PreferencesRepository _preferencesRepository;


        public AcumaticaOrderGet(
                AcumaticaOrderRepository orderRepository,
                AcumaticaTimeZoneService timeZoneService,
                AcumaticaBatchRepository batchStateRepository,
                AcumaticaHttpConfig acumaticaHttpConfig,
                PreferencesRepository preferencesRepository,
                SalesOrderClient salesOrderClient,
                ShipmentClient shipmentClient)
        {
            _orderRepository = orderRepository;
            _timeZoneService = timeZoneService;
            _batchStateRepository = batchStateRepository;
            _acumaticaHttpConfig = acumaticaHttpConfig;
            _salesOrderClient = salesOrderClient;
            _preferencesRepository = preferencesRepository;
            _shipmentClient = shipmentClient;
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
                var orders = 
                    _salesOrderClient.RetrieveUpdatedSalesOrders(
                        lastModifiedMin, page, pageSize, expand:Expand.Shipments_ShippingSettings);

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
                existingData.AcumaticaStatus = order.Status.value;
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
                var exists = _orderRepository.SoShipmentExists(
                        salesOrderRecord.Id, shipment.ShipmentNbr.value, shipment.InvoiceNbr.value);

                if (exists)
                {
                    continue;
                }
                if (shipment.Status.value != SoShipmentStatus.Complete)
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
                record.DateCreated = DateTime.UtcNow;
                record.LastUpdated = DateTime.UtcNow;

                _orderRepository.InsertSoShipmentInvoice(record);
            }
        }

        public void PopulateSoShipments(AcumaticaSalesOrder salesOrderRecord)
        {
            var soShipments = _orderRepository.RetrieveSoShipments(salesOrderRecord.Id);

            foreach (var soShipment in soShipments)
            {
                if (soShipment.NeedShipmentGet == false)
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
                    soShipment.AcumaticaTrackingNbr = trackingNumber;

                    _orderRepository.SaveChanges();
                }

                if (soShipment.AcumaticaInvoiceAmount == null || soShipment.AcumaticaInvoiceTax == null)
                {
                    var invoiceJson = 
                        _salesOrderClient.RetrieveSalesOrderInvoiceAndTaxes(
                            soShipment.AcumaticaInvoiceNbr, soShipment.AcumaticaInvoiceType);

                    var invoice = invoiceJson.DeserializeFromJson<SalesInvoice>();

                    soShipment.AcumaticaInvoiceAmount = (decimal)invoice.Amount.value;
                    soShipment.AcumaticaInvoiceTax = (decimal) invoice.TaxDetails.Sum(x => x.TaxAmount.value);

                    _orderRepository.SaveChanges();
                }

                soShipment.NeedShipmentGet = false;
                _orderRepository.SaveChanges();
            }
        }


        // TODO - PHASE 2 - Specifically for instances where the Sales Order in Acumatica was loaded
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
    }
}

