using System.Collections.Generic;
using System.Linq;
using Monster.Acumatica.Api;
using Monster.Acumatica.Api.Common;
using Monster.Acumatica.Api.SalesOrder;
using Monster.Acumatica.Api.Shipment;
using Monster.Middle.Persist.Multitenant;
using Monster.Middle.Persist.Multitenant.Acumatica;
using Monster.Middle.Persist.Multitenant.Shopify;
using Monster.Middle.Persist.Multitenant.Sync;
using Monster.Middle.Processes.Orders.Workers.Model;
using Push.Foundation.Utilities.Json;
using Push.Shopify.Api.Order;



namespace Monster.Middle.Processes.Orders.Workers
{
    public class AcumaticaRefundSync
    {
        private readonly SyncOrderRepository _syncOrderRepository;
        private readonly SyncInventoryRepository _syncInventoryRepository;
        private readonly ShopifyOrderRepository _shopifyOrderRepository;
        
        private readonly AcumaticaShipmentPull _acumaticaShipmentPull;
        private readonly ShipmentClient _shipmentClient;
        private readonly SalesOrderClient _salesOrderClient;
        
        public AcumaticaRefundSync(
                    SyncOrderRepository syncOrderRepository,
                    SyncInventoryRepository syncInventoryRepository,
                    ShopifyOrderRepository shopifyOrderRepository,
                    AcumaticaShipmentPull acumaticaShipmentPull,
                    ShipmentClient shipmentClient, 
                    SalesOrderClient salesOrderClient)
        {
            _syncOrderRepository = syncOrderRepository;
            _shopifyOrderRepository = shopifyOrderRepository;
            _acumaticaShipmentPull = acumaticaShipmentPull;
            _shipmentClient = shipmentClient;
            _salesOrderClient = salesOrderClient;
            _syncInventoryRepository = syncInventoryRepository;
        }


        public void Run()
        {
            var refunds = _syncOrderRepository.RetrieveRefundsNotSynced();

            foreach (var refund in refunds)
            {
                // 1) Validate existing of Inventory
                // 2) ...
            }
        }
        
        
        private void SyncFulfillmentWithAcumatica(
                        UsrShopifyFulfillment fulfillmentRecord)
        {
            var shopifyOrderRecord 
                = _syncOrderRepository
                        .RetrieveShopifyOrder(fulfillmentRecord.ShopifyOrderId);
            
            var shopifyOrder 
                = shopifyOrderRecord.ShopifyJson.DeserializeToOrder();
            
            var fulfillment
                = shopifyOrder
                    .fulfillments
                    .FirstOrDefault(x => x.id == fulfillmentRecord.ShopifyFulfillmentId);
            
            // Isolate the corresponding Acumatica Sales Order and Customer
            var salesOrderRecord = shopifyOrderRecord.MatchingSalesOrder();
            var customerNbr 
                = salesOrderRecord.UsrAcumaticaCustomer.AcumaticaCustomerId;
            
            // Isolate the Warehouse
            var locationRecord =
                    _syncInventoryRepository
                        .RetrieveLocation(fulfillment.location_id);
            var warehouse = locationRecord.MatchedWarehouse();

            // Create the Shipment API payload
            var shipment = new Shipment();
            shipment.Type = "Shipment".ToValue();
            shipment.Status = "Open".ToValue();
            shipment.Hold = false.ToValue();
            shipment.CustomerID = customerNbr.ToValue();
            shipment.WarehouseID = warehouse.AcumaticaWarehouseId.ToValue();
            shipment.Details = new List<ShipmentDetail>();
            shipment.ControlQty = ((double) fulfillment.ControlQuantity).ToValue();

            // Build out the Shipment Detail from the Shopify Fulfillment Line Items
            foreach (var line in fulfillment.line_items)
            {
                var variantRecord =
                    _syncInventoryRepository
                        .RetrieveVariant(line.variant_id, line.sku);

                var stockItemId
                    = variantRecord.MatchedStockItem().ItemId;
                
                var detail = new ShipmentDetail();
                detail.OrderNbr
                    = salesOrderRecord.AcumaticaOrderNbr.ToValue();

                detail.OrderType = "SO".ToValue();
                detail.InventoryID = stockItemId.ToValue();
                detail.WarehouseID = warehouse.AcumaticaWarehouseId.ToValue();
                detail.ShippedQty = 
                    ((double)line.fulfillable_quantity).ToValue();

                shipment.Details.Add(detail);
            }

            // Write the Shipment to Acumatica via API
            var resultJson
                = _shipmentClient.AddShipment(shipment.SerializeToJson());

            var resultShipment
                = resultJson.DeserializeFromJson<Shipment>();

            using (var transaction = _syncInventoryRepository.BeginTransaction())
            {
                // Create Monster footprint of Shipment
                var shipmentRecord
                    = _acumaticaShipmentPull
                        .UpsertShipmentToPersist(resultShipment, isCreatedByMonster: true);

                // NOTE - every Shopify Fulfillment will create
                // .. exactly one Acumatica Shipment, thus we can do this
                var shipmentSoRecord
                    = shipmentRecord.UsrAcumaticaShipmentDetails.First();

                _syncOrderRepository
                    .InsertShipmentDetailSync(fulfillmentRecord, shipmentSoRecord);

                transaction.Commit();
            }
        }

        
        public void CreateInvoiceForShipment(UsrAcumaticaShipment shipmentRecord)
        {
            var shipment = shipmentRecord.ToAcuObject();
            var customerId = shipment.CustomerID.value;
            
            var invoice = new SalesInvoicePayload();
            invoice.CustomerID = customerId.ToValue();
            invoice.Type = "Invoice".ToValue();

            foreach (var shipmentDetail in shipment.Details)
            {
                var invoiceDetail = new SalesInvoiceDetailsPayload();
                invoiceDetail.ShipmentNbr = shipment.ShipmentNbr.Copy();
                invoiceDetail.OrderNbr = shipmentDetail.OrderNbr.Copy();
                invoiceDetail.OrderType = shipmentDetail.OrderType.Copy();

                invoice.Details.Add(invoiceDetail);
            }

            var result = _salesOrderClient.AddInvoice(invoice.SerializeToJson());
            var resultInvoice = result.DeserializeFromJson<SalesInvoicePayload>();

            // Acumatica does not allow you split up Shipments for Invoicing
            shipmentRecord.AcumaticaInvoiceRefNbr = resultInvoice.ReferenceNbr.value;
            _syncOrderRepository.Entities.SaveChanges();
        }
    }
}

