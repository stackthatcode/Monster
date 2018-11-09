using System;
using System.Linq;
using Monster.Acumatica.Api.Shipment;
using Monster.Middle.Persist.Multitenant.Acumatica;
using Monster.Middle.Persist.Multitenant.Etc;
using Monster.Middle.Persist.Multitenant.Shopify;
using Push.Foundation.Utilities.Json;
using Push.Foundation.Utilities.Logging;
using Push.Shopify.Api;
using Push.Shopify.Api.Inventory;

namespace Monster.Middle.Processes.Orders.Workers
{
    public class ShopifyFulfillmentSync
    {
        private readonly BatchStateRepository _batchStateRepository;
        private readonly IPushLogger _logger;
        private readonly OrderApi _orderApi;
        private readonly ShopifyOrderRepository _shopifyOrderRepository;
        private readonly AcumaticaOrderRepository _acumaticaOrderRepository;

        // Possibly expand - this is a one-time thing...
        public const int InitialBatchStateFudgeMin = -15;


        public ShopifyFulfillmentSync(
                IPushLogger logger,
                BatchStateRepository batchStateRepository,
                OrderApi orderApi,
                ShopifyOrderRepository shopifyOrderRepository,
                AcumaticaOrderRepository acumaticaOrderRepository)
        {
            _logger = logger;
            _batchStateRepository = batchStateRepository;
            _orderApi = orderApi;
            _shopifyOrderRepository = shopifyOrderRepository;
            _acumaticaOrderRepository = acumaticaOrderRepository;
        }


        public void Run()
        {
            // Shipments that have Sales Orders, but no matching Shopify Fulfillment
            var shipments =
                _acumaticaOrderRepository.RetrieveShipmentsUnsynced();

            // TODO - more Acumatica Status filtering
            var correctedShipments =
                shipments
                    .Where(x => x.AcumaticaStatus != "Hold")
                    .ToList();

            foreach (var shipment in shipments)
            {
                //var shipmentObject 
                //    = shipment.AcumaticaJson.DeserializeFromJson<Shipment>();
                

                //var salesOrder = 


                //var quantity = (int)warehouseDetail.AcumaticaQtyOnHand;

                //var location =
                //    warehouseDetail
                //        .UsrAcumaticaWarehouse
                //        .UsrShopifyLocation;

                //var shopifyLocationId = location.ShopifyLocationId;
                //var locationMonsterId = location.MonsterId;

                //var shopifyInventoryItemId =
                //    warehouseDetail
                //        .UsrAcumaticaStockItem
                //        .UsrShopifyVariant
                //        .ShopifyInventoryItemId;

                //var inventoryLevelData
                //    = warehouseDetail
                //        .UsrAcumaticaStockItem
                //        .UsrShopifyVariant
                //        .UsrShopifyInventoryLevels
                //        .First(x => x.LocationMonsterId == locationMonsterId);

                //var level = new InventoryLevel
                //{
                //    inventory_item_id = shopifyInventoryItemId,
                //    available = quantity,
                //    location_id = shopifyLocationId,
                //};

                //var levelJson = level.SerializeToJson();
                //var resultJson = _inventoryApi.SetInventoryLevels(levelJson);

                //// Flag Acumatica Warehouse Detail as synchronized
                //warehouseDetail.ShopifyIsSynced = true;

                //// Update Shopify Inventory Level records
                //inventoryLevelData.ShopifyAvailableQuantity = quantity;
                //inventoryLevelData.LastUpdated = DateTime.UtcNow;

                //_inventoryRepository.SaveChanges();
            }
        }
    }
}
