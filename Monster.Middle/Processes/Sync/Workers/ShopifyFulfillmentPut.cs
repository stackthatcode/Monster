using System;
using System.Collections.Generic;
using Monster.Acumatica.Api.Shipment;
using Monster.Middle.Misc.Logging;
using Monster.Middle.Persist.Instance;
using Monster.Middle.Processes.Acumatica.Persist;
using Monster.Middle.Processes.Shopify.Persist;
using Monster.Middle.Processes.Sync.Model.Misc;
using Monster.Middle.Processes.Sync.Model.Orders;
using Monster.Middle.Processes.Sync.Persist;
using Monster.Middle.Processes.Sync.Persist.Matching;
using Monster.Middle.Processes.Sync.Status;
using Push.Foundation.Utilities.Json;
using Push.Shopify.Api;
using Push.Shopify.Api.Inventory;
using Push.Shopify.Api.Order;

namespace Monster.Middle.Processes.Sync.Workers
{
    public class ShopifyFulfillmentPut
    {
        private readonly FulfillmentApi _fulfillmentApi;
        private readonly ShopifyOrderRepository _shopifyOrderRepository;
        private readonly AcumaticaOrderRepository _orderRepository;
        private readonly SyncOrderRepository _syncOrderRepository;
        private readonly SyncInventoryRepository _syncInventoryRepository;
        private readonly ExecutionLogService _logService;
        private readonly FulfillmentStatusService _fulfillmentStatusService;

        public ShopifyFulfillmentPut(
                ShopifyOrderRepository shopifyOrderRepository,
                AcumaticaOrderRepository orderRepository, 
                SyncOrderRepository syncOrderRepository,
                SyncInventoryRepository syncInventoryRepository,
                FulfillmentApi fulfillmentApi, 
                ExecutionLogService logService, 
                FulfillmentStatusService fulfillmentStatusService)
        {
            _shopifyOrderRepository = shopifyOrderRepository;
            _orderRepository = orderRepository;
            _syncOrderRepository = syncOrderRepository;
            _syncInventoryRepository = syncInventoryRepository;
            _fulfillmentApi = fulfillmentApi;
            _logService = logService;
            _fulfillmentStatusService = fulfillmentStatusService;
        }


        public void Run()
        {
            var salesOrderRefs = _orderRepository.RetrieveUnsyncedShipmentSalesOrderRefs();
            
            foreach (var salesOrderRef in salesOrderRefs)
            {
                var syncReadiness 
                    = _fulfillmentStatusService.IsReadyToSyncWithShopify(salesOrderRef);

                if (syncReadiness.IsReady)
                {
                    var content = LogBuilder.CreateShopifyFulfillment(salesOrderRef);
                    _logService.Log(content);
                    PushFulfillmentToShopify(salesOrderRef);
                }
            }
        }


        public void PushFulfillmentToShopify(
                    AcumaticaShipmentSalesOrderRef shipmentSalesOrderRef)
        {
            var orderRecord =
                _syncOrderRepository.RetrieveSalesOrder(shipmentSalesOrderRef.AcumaticaOrderNbr);

            if (!orderRecord.IsFromShopify())
            {
                return;
            }

            var shopifyOrderRecord = orderRecord.MatchingShopifyOrder();
            var shopifyOrder = shopifyOrderRecord.ShopifyJson.DeserializeToOrder();            
            var shipmentRecord = shipmentSalesOrderRef.AcumaticaShipment;
            var shipment 
                = shipmentRecord.AcumaticaJson.DeserializeFromJson<Shipment>();
            
            var location = RetrieveMatchingLocation(shipment);

            // Line Items
            var details = shipment.DetailByOrder(orderRecord.AcumaticaOrderNbr);
            var fulfillment = new Fulfillment();
            fulfillment.line_items = new List<LineItem>();
            fulfillment.location_id = location.id;
            fulfillment.tracking_number = shipmentRecord.AcumaticaTrackingNbr;

            // Build the Detail
            foreach (var detail in details)
            {
                var stockItem 
                    = _syncInventoryRepository.RetrieveStockItem(detail.InventoryID.value);
                var variant = stockItem.MatchedVariant();
                
                var shopifyLineItem = shopifyOrder.LineItem(variant.ShopifySku);
                var quantity = detail.ShippedQty.value;

                var fulfillmentDetail = new LineItem()
                {                    
                    id = shopifyLineItem.id,
                    quantity = (int)quantity,
                };

                fulfillment.line_items.Add(fulfillmentDetail);
            }

            // Write the Fulfillment to the Shopify API
            var parent = new FulfillmentParent() { fulfillment = fulfillment };
            var result = 
                _fulfillmentApi.Insert(
                    shopifyOrderRecord.ShopifyOrderId, parent.SerializeToJson());
            var resultFulfillmentParent = result.DeserializeFromJson<FulfillmentParent>();
            
            // Save the result
            var fulfillmentRecord = new ShopifyFulfillment();
            fulfillmentRecord.OrderMonsterId = shopifyOrderRecord.Id;
            fulfillmentRecord.ShopifyOrderId = shopifyOrder.id;
            fulfillmentRecord.ShopifyFulfillmentId 
                    = resultFulfillmentParent.fulfillment.id;

            fulfillmentRecord.ShopifyStatus = resultFulfillmentParent.fulfillment.status;
            fulfillmentRecord.DateCreated = DateTime.UtcNow;
            fulfillmentRecord.LastUpdated = DateTime.UtcNow;

            using (var transaction = _syncInventoryRepository.BeginTransaction())
            {
                _shopifyOrderRepository.InsertFulfillment(fulfillmentRecord);
                _syncOrderRepository.InsertShipmentDetailSync(fulfillmentRecord, shipmentSalesOrderRef);

                var log = $"Created Shopify Order #{shopifyOrder.order_number} Fulfillment " +
                          $"from Acumatica Shipment {shipment.ShipmentNbr.value}";

                _logService.Log(log);
                transaction.Commit();
            }
        }

        public Location RetrieveMatchingLocation(Shipment shipment)
        {
            var warehouse = _syncInventoryRepository.RetrieveWarehouse(shipment.WarehouseID.value);
            var locationRecord = warehouse.MatchedLocation();
            var location = locationRecord.ShopifyJson.DeserializeFromJson<Location>();
            return location;
        }
    }
}
