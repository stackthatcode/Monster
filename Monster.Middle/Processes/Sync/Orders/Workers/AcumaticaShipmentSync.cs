using System;
using System.Collections.Generic;
using System.Linq;
using Monster.Acumatica.Api;
using Monster.Acumatica.Api.Common;
using Monster.Acumatica.Api.Shipment;
using Monster.Middle.Persist.Multitenant;
using Monster.Middle.Processes.Acumatica.Persist;
using Monster.Middle.Processes.Acumatica.Workers;
using Monster.Middle.Processes.Shopify.Persist;
using Monster.Middle.Processes.Sync.Extensions;
using Monster.Middle.Processes.Sync.Inventory;
using Monster.Middle.Processes.Sync.Orders.Model;
using Push.Foundation.Utilities.Json;
using Push.Shopify.Api.Order;


namespace Monster.Middle.Processes.Sync.Orders.Workers
{
    [Obsolete]
    public class AcumaticaShipmentSync
    {
        private readonly SyncOrderRepository _orderRepository;
        private readonly SyncInventoryRepository _syncInventoryRepository;
        private readonly ShopifyOrderRepository _shopifyOrderRepository;
        
        private readonly AcumaticaShipmentPull _acumaticaShipmentPull;
        private readonly ShipmentClient _shipmentClient;
        private readonly StateRepository _stateRepository;
        private readonly ExecutionLogRepository _logRepository;

        public AcumaticaShipmentSync(
                    SyncOrderRepository orderRepository,
                    SyncInventoryRepository syncInventoryRepository,
                    ShopifyOrderRepository shopifyOrderRepository,
                    AcumaticaShipmentPull acumaticaShipmentPull,
                    ShipmentClient shipmentClient, 
                    StateRepository stateRepository, 
                    ExecutionLogRepository logRepository)
        {
            _orderRepository = orderRepository;
            _shopifyOrderRepository = shopifyOrderRepository;
            _acumaticaShipmentPull = acumaticaShipmentPull;
            _shipmentClient = shipmentClient;
            _stateRepository = stateRepository;
            _logRepository = logRepository;
            _syncInventoryRepository = syncInventoryRepository;            
        }


        public void RunShipments()
        {
            var fulfillments = _orderRepository.RetrieveFulfillmentsNotSynced();

            foreach (var fulfillment in fulfillments)
            {
                var acumaticaOrder = fulfillment.UsrShopifyOrder.MatchingSalesOrder();

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
                    WriteShipmentToAcumatica(fulfillment);
                }
            }
        }
        
        public void RunByShopifyId(long shopifyFulfillmentId)
        {
            var fulfillment 
                = _shopifyOrderRepository
                    .RetreiveFulfillment(shopifyFulfillmentId);

            WriteShipmentToAcumatica(fulfillment);
        }
        
        private ShipmentSyncReadiness 
                    IsReadyToSyncWithAcumatica(UsrShopifyFulfillment fulfillmentRecord)
        {
            var output = new ShipmentSyncReadiness();

            var shopifyOrderRecord
                = _orderRepository
                    .RetrieveShopifyOrder(fulfillmentRecord.ShopifyOrderId);

            var shopifyOrder = shopifyOrderRecord.ShopifyJson.DeserializeToOrder();

            var shopifyFulfillment
                = shopifyOrder.Fulfillment(fulfillmentRecord.ShopifyFulfillmentId);
            
            // Isolate the Warehouse
            var locationRecord =
                _syncInventoryRepository.RetrieveLocation(shopifyFulfillment.location_id);

            var warehouseId = locationRecord.MatchedWarehouse().AcumaticaWarehouseId;

            foreach (var line in shopifyFulfillment.line_items)
            {
                var variantRecord =
                    _syncInventoryRepository.RetrieveVariant(line.variant_id.Value, line.sku);

                var stockItemId = variantRecord.MatchedStockItem().ItemId;

                // Get Warehouse Details
                var stockItem = _syncInventoryRepository.RetrieveStockItem(stockItemId);
                var warehouseDetail = stockItem.WarehouseDetail(warehouseId);

                if (line.quantity > warehouseDetail.AcumaticaQtyOnHand)
                {
                    output.StockItemsWithInsuffientInventory.Add(line.sku);
                }
            }

            return output;
        }
        
        private void WriteShipmentToAcumatica(UsrShopifyFulfillment fulfillmentRecord)
        {
            var shopifyOrderRecord 
                = _orderRepository.RetrieveShopifyOrder(fulfillmentRecord.ShopifyOrderId);
            
            var shopifyOrder = shopifyOrderRecord.ShopifyJson.DeserializeToOrder();            
            var fulfillment = shopifyOrder.Fulfillment(fulfillmentRecord.ShopifyFulfillmentId);

            var location = _syncInventoryRepository.RetrieveLocation(fulfillment.location_id);
            var warehouseId = location.MatchedWarehouse().AcumaticaWarehouseId;

            // Isolate the corresponding Acumatica Sales Order and Customer
            var salesOrderRecord = shopifyOrderRecord.MatchingSalesOrder();
            var customerNbr = salesOrderRecord.UsrAcumaticaCustomer.AcumaticaCustomerId;

            // Create the Shipment API payload
            var shipment = new Shipment();
            shipment.Type = AcumaticaConstants.ShipmentType.ToValue();
            shipment.Status = Acumatica.Persist.Status.Open.ToValue();
            shipment.Hold = false.ToValue();
            shipment.CustomerID = customerNbr.ToValue();
            shipment.WarehouseID = warehouseId.ToValue();
            shipment.Details = new List<ShipmentDetail>();
            shipment.ControlQty = ((double) fulfillment.ControlQuantity).ToValue();

            // Build out the Shipment Detail from the Shopify Fulfillment Line Items
            foreach (var line in fulfillment.line_items)
            {
                var variantRecord =
                    _syncInventoryRepository
                        .RetrieveVariant(line.variant_id.Value, line.sku);

                var stockItemId = variantRecord.MatchedStockItem().ItemId;
                
                var detail = new ShipmentDetail();
                detail.OrderNbr = salesOrderRecord.AcumaticaOrderNbr.ToValue();
                detail.OrderType = SalesOrderType.SO.ToValue();
                detail.InventoryID = stockItemId.ToValue();
                detail.WarehouseID = warehouseId.ToValue();
                detail.ShippedQty = ((double)line.quantity).ToValue();

                shipment.Details.Add(detail);
            }

            // Write the Shipment to Acumatica via API
            var resultJson
                = _shipmentClient.WriteShipment(shipment.SerializeToJson());

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
                    = shipmentRecord.UsrAcumaticaShipmentSalesOrderRefs.First();

                _orderRepository
                    .InsertShipmentDetailSync(fulfillmentRecord, shipmentSoRecord);

                var log = 
                    $"Created Shipment {resultShipment.ShipmentNbr.value} in Acumatica from "+ 
                    $"Shopify Order #{shopifyOrder.order_number}";

                _logRepository.InsertExecutionLog(log);
                
                transaction.Commit();
            }
        }
        
        public void RunConfirmShipments()
        {
            var shipmentRecords =
                _orderRepository.RetrieveShipmentsByMonsterNotConfirmed();

            foreach (var shipmentRecord in shipmentRecords)
            {
                ConfirmShipment(shipmentRecord);
            }
        }

        public void ConfirmShipment(UsrAcumaticaShipment shipmentRecord)
        {
            var payload = new ShipmentConfirmation(shipmentRecord.AcumaticaShipmentNbr);

            var result = _shipmentClient.ConfirmShipment(payload.SerializeToJson());
            shipmentRecord.AcumaticaStatus = "Confirmed";
            _syncInventoryRepository.SaveChanges();

            var log = $"Confirmed Shipment {shipmentRecord.AcumaticaShipmentNbr} in Acumatica";
            _logRepository.InsertExecutionLog(log);
        }


        public void RunSingleInvoicePerShipmentSalesRef()
        {
            var shipmentRecords = 
                _orderRepository
                    .RetrieveShipmentsByMonsterWithNoInvoice();

            foreach (var shipmentRecord in shipmentRecords)
            {
                CreateInvoiceForShipment(shipmentRecord);
            }
        }


        // ON HOLD - pending any usage of Shopify-based Fulfillment
        public void CreateInvoiceForShipment(UsrAcumaticaShipment shipmentRecord)
        {
            //var shipment = shipmentRecord.ToAcuObject();
            //var customerId = shipment.CustomerID.value;
            
            //var invoice = new SalesInvoiceWrite();
            //invoice.CustomerID = customerId.ToValue();
            //invoice.Type = "Invoice".ToValue();

            //foreach (var shipmentDetail in shipment.Details)
            //{
            //    var invoiceDetail = new SalesInvoiceDetailsUpdate();
            //    invoiceDetail.ShipmentNbr = shipment.ShipmentNbr.Copy();
            //    invoiceDetail.OrderNbr = shipmentDetail.OrderNbr.Copy();
            //    invoiceDetail.OrderType = shipmentDetail.OrderType.Copy();

            //    invoice.Details.Add(invoiceDetail);
            //}

            //// *** TODO - Using Stubbing code, or create a Record
            //var result = _salesOrderClient.AddInvoice(invoice.SerializeToJson());
            //var resultInvoice = result.DeserializeFromJson<SalesInvoiceWrite>();

            //// Acumatica does not allow you split up Shipments for Invoicing
            //shipmentRecord.AcumaticaInvoiceNbr = resultInvoice.ReferenceNbr.value;
            //_orderRepository.Entities.SaveChanges();

            //var log = $"Created Invoice for Shipment {shipmentRecord.AcumaticaShipmentNbr} in Acumatica";
            //_jobRepository.InsertExecutionLog(log);
        }
    }
}

