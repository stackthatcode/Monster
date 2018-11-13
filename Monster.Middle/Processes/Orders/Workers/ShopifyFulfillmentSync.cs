using System.Collections.Generic;
using System.Linq;
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
            var shipmentsRecords =
                _acumaticaOrderRepository.RetrieveShipmentsUnsynced();

            // TODO - more Acumatica Status filtering - 
            // 
            var filteredShipmentRecords =
                shipmentsRecords
                    .Where(x => x.AcumaticaStatus != "Hold")
                    .ToList();

            foreach (var shipmentRecord in filteredShipmentRecords)
            {
                var shipment = 
                    shipmentRecord
                        .AcumaticaJson
                        .DeserializeFromJson<Shipment>();

                foreach (var orderNbr in shipment.UniqueOrderNbrs)
                {
                    var order = 
                        _syncOrderRepository.RetrieveSalesOrder(orderNbr);

                    if (!order.IsFromShopify())
                    {
                        continue;
                    }
                    

                }
            }
        }

        public void PushFulfillmentToShopify(
                    Shipment shipment, UsrAcumaticaSalesOrder orderRecord)
        {
            var shopifyOrderRecord = orderRecord.MatchingShopifyOrder();
            var shopifyOrder = shopifyOrderRecord.ShopifyJson.DeserializeToOrder();

            // Location
            var warehouse =
                _syncInventoryRepository
                    .RetrieveWarehouse(shipment.WarehouseID.value);
            var locationRecord = warehouse.MatchedLocation();
            var location = locationRecord.ShopifyJson.DeserializeFromJson<Location>();

            // Line Items
            var details = shipment.DetailByOrder(orderRecord.AcumaticaSalesOrderId);

            var fulfillment = new Fulfillment();
            fulfillment.line_items = new List<LineItem>();
            fulfillment.location_id = location.id;

            // TODO - add Tracking Number

            foreach (var detail in details)
            {
                // TODO - for now, just use the SKU
                //var stockItem
                //    = _syncInventoryRepository
                //        .RetrieveStockItem(detail.InventoryID.value);
                //var variant = stockItem.MatchedVariant();

                var shopifySku = detail.InventoryID.value;

                var shopifyLineItem = shopifyOrder.LineItem(shopifySku);

                var quantity = detail.ShippedQty.value;

                var fulfillmentDetail = new LineItem()
                {                    
                    id = shopifyLineItem.id
                };

                fulfillment.line_items.Add(fulfillmentDetail);
            }

            var parent = new FulfillmentParent()
            {
                fulfillment = fulfillment
            };

            _fulfillmentApi.Insert(
                shopifyOrderRecord.ShopifyOrderId, parent.SerializeToJson());
        }
    }
}
