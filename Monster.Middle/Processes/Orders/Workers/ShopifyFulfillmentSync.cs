using System;
using System.Collections.Generic;
using Monster.Acumatica.Api.Shipment;
using Monster.Middle.Persist.Multitenant;
using Monster.Middle.Persist.Multitenant.Acumatica;
using Monster.Middle.Persist.Multitenant.Etc;
using Monster.Middle.Persist.Multitenant.Shopify;
using Monster.Middle.Persist.Multitenant.Sync;
using Push.Foundation.Utilities.Json;
using Push.Foundation.Utilities.Logging;
using Push.Shopify.Api;
using Push.Shopify.Api.Inventory;
using Push.Shopify.Api.Order;


namespace Monster.Middle.Processes.Orders.Workers
{
    public class ShopifyFulfillmentSync
    {
        private readonly BatchStateRepository _batchStateRepository;
        private readonly IPushLogger _logger;
        private readonly FulfillmentApi _fulfillmentApi;
        private readonly ShopifyOrderRepository _shopifyOrderRepository;
        private readonly AcumaticaOrderRepository _acumaticaOrderRepository;
        private readonly SyncOrderRepository _syncOrderRepository;
        private readonly SyncInventoryRepository _syncInventoryRepository;

        // Possibly expand - this is a one-time thing...
        public const int InitialBatchStateFudgeMin = -15;


        public ShopifyFulfillmentSync(
                IPushLogger logger,
                BatchStateRepository batchStateRepository,
                ShopifyOrderRepository shopifyOrderRepository,
                AcumaticaOrderRepository acumaticaOrderRepository, 
                SyncOrderRepository syncOrderRepository,
                SyncInventoryRepository syncInventoryRepository,
                FulfillmentApi fulfillmentApi)
        {
            _logger = logger;
            _batchStateRepository = batchStateRepository;
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
                
                foreach (var shipmentSo in shipmentRecord.UsrAcumaticaShipmentDetails)
                {
                    PushFulfillmentToShopify(shipmentSo);
                }
            }
        }

        public void PushFulfillmentToShopify(UsrAcumaticaShipmentDetail shipmentSoRecord)
        {
            var orderRecord =
                _syncOrderRepository
                    .RetrieveSalesOrder(shipmentSoRecord.AcumaticaOrderNbr);

            if (!orderRecord.IsFromShopify())
            {
                return;
            }

            var shopifyOrderRecord = orderRecord.MatchingShopifyOrder();
            var shopifyOrder = shopifyOrderRecord.ShopifyJson.DeserializeToOrder();

            var shipment
                = shipmentSoRecord
                    .UsrAcumaticaShipment
                    .AcumaticaJson.DeserializeFromJson<Shipment>();

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
                _syncOrderRepository.InsertShipmentDetailSync(fulfillmentRecord, shipmentSoRecord);
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
