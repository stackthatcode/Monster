﻿using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
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
    public class AcumaticaShipmentSync
    {
        private readonly SyncOrderRepository _syncOrderRepository;
        private readonly SyncInventoryRepository _syncInventoryRepository;
        private readonly ShopifyOrderRepository _shopifyOrderRepository;
        
        private readonly AcumaticaShipmentPull _acumaticaShipmentPull;
        private readonly ShipmentClient _shipmentClient;
        private readonly SalesOrderClient _salesOrderClient;
        
        public AcumaticaShipmentSync(
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


        public void RunShipments()
        {
            var fulfillments 
                    = _syncOrderRepository.RetrieveFulfillmentsNotSynced();

            foreach (var fulfillment in fulfillments)
            {
                var acumaticaOrder 
                        = fulfillment.UsrShopifyOrder.MatchingSalesOrder();

                if (acumaticaOrder == null)
                {
                    continue;
                }

                if (!acumaticaOrder.IsReadyForShipment())
                {
                    continue;
                }

                var readiness = IsReadyToSyncWithAcumatica(fulfillment);

                if (readiness.IsReady)
                {
                    SyncFulfillmentWithAcumatica(fulfillment);
                }
            }
        }
        
        public void RunByShopifyId(long shopifyFulfillmentId)
        {
            var fulfillment 
                = _shopifyOrderRepository
                    .RetreiveFulfillment(shopifyFulfillmentId);

            SyncFulfillmentWithAcumatica(fulfillment);
        }
        
        private ShipmentSyncReadiness 
                IsReadyToSyncWithAcumatica(
                        UsrShopifyFulfillment fulfillmentRecord)
        {
            var output = new ShipmentSyncReadiness();

            var shopifyOrderRecord
                = _syncOrderRepository
                    .RetrieveShopifyOrder(fulfillmentRecord.ShopifyOrderId);

            var shopifyOrder
                = shopifyOrderRecord.ShopifyJson.DeserializeToOrder();

            var shopifyFulfillment
                = shopifyOrder.Fulfillment(fulfillmentRecord.ShopifyFulfillmentId);
            
            // Isolate the Warehouse
            var locationRecord =
                _syncInventoryRepository
                    .RetrieveLocation(shopifyFulfillment.location_id);

            var warehouseId = locationRecord.MatchedWarehouse().AcumaticaWarehouseId;

            // Create the Shipment API payload            
            foreach (var line in shopifyFulfillment.line_items)
            {
                var variantRecord =
                    _syncInventoryRepository
                        .RetrieveVariant(line.variant_id, line.sku);

                var stockItemId = variantRecord.MatchedStockItem().ItemId;

                // Get Warehouse Details
                var stockItem =
                    _syncInventoryRepository.RetrieveStockItem(stockItemId);

                var warehouseDetail = stockItem.WarehouseDetail(warehouseId);

                if (line.quantity > warehouseDetail.AcumaticaQtyOnHand)
                {
                    output.SkuWithInsuffientInventory.Add(line.sku);
                }
            }

            return output;
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


        public void RunSingleInvoicePerShipment()
        {
            var shipmentRecords = 
                _syncOrderRepository
                    .RetrieveConfirmedShipmentsCreatedByMonster();

            foreach (var shipmentRecord in shipmentRecords)
            {
                CreateAcumaticaInvoiceFromMonsterShipment(shipmentRecord);
            }
        }

        public void CreateAcumaticaInvoiceFromMonsterShipment(UsrAcumaticaShipment shipmentRecord)
        {
            var shipment =
                shipmentRecord.AcumaticaJson.DeserializeFromJson<Shipment>();

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

            shipmentRecord.AcumaticaInvoiceRefNbr 
                    = resultInvoice.ReferenceNbr.value;
            _syncOrderRepository.Entities.SaveChanges();
        }
    }
}
