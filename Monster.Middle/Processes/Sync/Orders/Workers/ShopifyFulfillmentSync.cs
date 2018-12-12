using System;
using System.Collections.Generic;
using Monster.Acumatica.Api.Shipment;
using Monster.Middle.Persist.Multitenant;
using Monster.Middle.Processes.Acumatica.Persist;
using Monster.Middle.Processes.Shopify.Persist;
using Monster.Middle.Processes.Sync.Extensions;
using Monster.Middle.Processes.Sync.Persist;
using Push.Foundation.Utilities.Json;
using Push.Foundation.Utilities.Logging;
using Push.Shopify.Api;
using Push.Shopify.Api.Inventory;
using Push.Shopify.Api.Order;

namespace Monster.Middle.Processes.Sync.Orders.Workers
{
    public class ShopifyFulfillmentSync
    {
        private readonly ShopifyBatchRepository _shopifyBatchRepository;
        private readonly IPushLogger _logger;
        private readonly JobRepository _jobRepository;
        private readonly FulfillmentApi _fulfillmentApi;
        private readonly ShopifyOrderRepository _shopifyOrderRepository;
        private readonly AcumaticaOrderRepository _acumaticaOrderRepository;
        private readonly SyncOrderRepository _syncOrderRepository;
        private readonly SyncInventoryRepository _syncInventoryRepository;

        // Possibly expand - this is a one-time thing...
        public const int InitialBatchStateFudgeMin = -15;


        public ShopifyFulfillmentSync(
                IPushLogger logger,
                JobRepository jobRepository,
                ShopifyBatchRepository shopifyBatchRepository,
                ShopifyOrderRepository shopifyOrderRepository,
                AcumaticaOrderRepository acumaticaOrderRepository, 
                SyncOrderRepository syncOrderRepository,
                SyncInventoryRepository syncInventoryRepository,
                FulfillmentApi fulfillmentApi)
        {
            _logger = logger;
            _jobRepository = jobRepository;
            _shopifyBatchRepository = shopifyBatchRepository;
            _shopifyOrderRepository = shopifyOrderRepository;
            _acumaticaOrderRepository = acumaticaOrderRepository;
            _syncOrderRepository = syncOrderRepository;
            _syncInventoryRepository = syncInventoryRepository;
            _fulfillmentApi = fulfillmentApi;
        }


        public void Run()
        {
            // Shipments that have Sales Orders, but no matching Shopify Fulfillment
            var shipmentIds =
                _acumaticaOrderRepository.RetrieveUnsyncedShipmentIds();

            foreach (var shipmentId in shipmentIds)
            {
                var shipmentRecord =
                    _acumaticaOrderRepository.RetrieveShipment(shipmentId);
                
                foreach (var shipmentSo in shipmentRecord.UsrAcumaticaShipmentSalesOrderRefs)
                {
                    PushFulfillmentToShopify(shipmentSo);
                }
            }
        }

        public void PushFulfillmentToShopify(
                UsrAcumaticaShipmentSalesOrderRef shipmentSalesOrderRef)
        {
            var orderRecord =
                _syncOrderRepository
                    .RetrieveSalesOrder(shipmentSalesOrderRef.AcumaticaOrderNbr);

            // TODO - This should've been filtered by the View
            if (!orderRecord.IsFromShopify())
            {
                return;
            }

            // TODO - Does this Order have Fulfillments that not synced with Acumatica?
            // ... if so, then we can't do this either

            var shopifyOrderRecord = orderRecord.MatchingShopifyOrder();
            var shopifyOrder = shopifyOrderRecord.ShopifyJson.DeserializeToOrder();

            // TODO - This should've been filtered by the View
            if (shipmentSalesOrderRef
                    .UsrAcumaticaShipment
                    .AcumaticaReleasedInvoiceNbr == null)
            {
                return;
            }

            var shipmentRecord = shipmentSalesOrderRef.UsrAcumaticaShipment;
            var shipment
                = shipmentRecord.AcumaticaJson.DeserializeFromJson<Shipment>();
            
            var location = RetrieveMatchingLocation(shipment);

            // Line Items
            var details = shipment.DetailByOrder(orderRecord.AcumaticaOrderNbr);

            var fulfillment = new Fulfillment();
            fulfillment.line_items = new List<LineItem>();
            fulfillment.location_id = location.id;
            // TODO - add Tracking Number...?

            // Build the Detail
            foreach (var detail in details)
            {
                // TODO - for now, just use the SKU... although beware of doing this!
                var shopifySku = detail.InventoryID.value;
                var shopifyLineItem = shopifyOrder.LineItem(shopifySku);
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
            var fulfillmentRecord = new UsrShopifyFulfillment();
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
                _jobRepository.InsertExecutionLog(log);

                transaction.Commit();
            }
        }

        public Location RetrieveMatchingLocation(Shipment shipment)
        {
            var warehouse =
                _syncInventoryRepository.RetrieveWarehouse(shipment.WarehouseID.value);
            var locationRecord = warehouse.MatchedLocation();
            var location = locationRecord.ShopifyJson.DeserializeFromJson<Location>();
            return location;
        }
    }
}
