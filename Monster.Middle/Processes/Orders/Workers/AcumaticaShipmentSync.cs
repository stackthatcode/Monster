﻿using System.Collections.Generic;
using System.Linq;
using Monster.Acumatica.Api;
using Monster.Acumatica.Api.Common;
using Monster.Acumatica.Api.Shipment;
using Monster.Middle.Persist.Multitenant;
using Monster.Middle.Persist.Multitenant.Acumatica;
using Monster.Middle.Persist.Multitenant.Extensions;
using Monster.Middle.Persist.Multitenant.Shopify;
using Monster.Middle.Persist.Multitenant.Sync;
using Push.Foundation.Utilities.Json;
using Push.Shopify.Api.Order;

namespace Monster.Middle.Processes.Orders.Workers
{
    public class AcumaticaShipmentSync
    {
        private readonly SyncOrderRepository _syncOrderRepository;
        private readonly SyncInventoryRepository _syncInventoryRepository;
        private readonly ShopifyOrderRepository _shopifyOrderRepository;
        private readonly ShopifyInventoryRepository _shopifyInventoryRepository;
        private readonly AcumaticaOrderRepository _acumaticaOrderRepository;

        private readonly AcumaticaShipmentPull _acumaticaShipmentPull;
        private readonly ShipmentClient _shipmentClient;
        
        public AcumaticaShipmentSync(
                    SyncOrderRepository syncOrderRepository,
                    ShopifyOrderRepository shopifyOrderRepository,
                    ShopifyInventoryRepository shopifyInventoryRepository,
                    AcumaticaOrderRepository acumaticaOrderRepository,
                    AcumaticaShipmentPull acumaticaShipmentPull,
                    ShipmentClient shipmentClient)
        {
            _syncOrderRepository = syncOrderRepository;
            _shopifyOrderRepository = shopifyOrderRepository;
            _shopifyInventoryRepository = shopifyInventoryRepository;
            _acumaticaOrderRepository = acumaticaOrderRepository;
            _acumaticaShipmentPull = acumaticaShipmentPull;
            _shipmentClient = shipmentClient;
        }


        public void Run()
        {
            var fulfillments 
                    = _syncOrderRepository.RetrieveFulfillmentsNotSynced();

            foreach (var fulfillment in fulfillments)
            {
                var acumaticaOrder = fulfillment.UsrShopifyOrder.AcumaticaSalesOrder();

                if (acumaticaOrder == null)
                {
                    continue;
                }

                if (!acumaticaOrder.IsReadyForShipment())
                {
                    continue;
                }

                SyncFulfillmentWithAcumatica(fulfillment);
            }
        }

        public void RunByShopifyId(long shopifyFulfillmentId)
        {
            var fulfillment 
                = _shopifyOrderRepository
                    .RetreiveFulfillment(shopifyFulfillmentId);

            SyncFulfillmentWithAcumatica(fulfillment);
        }

        private void SyncFulfillmentWithAcumatica(
                        UsrShopifyFulfillment fulfillmentRecord)
        {
            var shopifyOrder
                = fulfillmentRecord
                    .UsrShopifyOrder
                    .ShopifyJson
                    .DeserializeToOrder();
            
            var shopifyFulfillment
                = shopifyOrder
                    .fulfillments
                    .FirstOrDefault(x => x.id == fulfillmentRecord.ShopifyFulfillmentId);


            // Isolate the corresponding Acumatica Sales Order and Customer
            var salesOrderRecord 
                = _acumaticaOrderRepository
                        .RetrieveSalesOrderByShopify(fulfillmentRecord.OrderMonsterId);

            var customerNbr 
                = salesOrderRecord.UsrAcumaticaCustomer.AcumaticaCustomerId;


            // Isolate the Warehouse
            var locationRecord =
                    _syncInventoryRepository
                        .RetrieveLocation(shopifyFulfillment.location_id);
            var warehouse = locationRecord.MatchedWarehouse();

            // Create the Shipment API payload
            var shipment = new Shipment();
            shipment.Type = "Shipment".ToValue();
            shipment.Status = "Open".ToValue();
            shipment.CustomerID = customerNbr.ToValue();
            shipment.WarehouseID = warehouse.AcumaticaWarehouseId.ToValue();
            shipment.Details = new List<ShipmentDetail>();

            foreach (var line in shopifyFulfillment.line_items)
            {
                var variantRecord =
                    _shopifyInventoryRepository
                        .RetrieveVariant(line.variant_id, line.sku);

                var stockItemId
                    = variantRecord.MatchedStockItem().ItemId;
                
                var detail = new ShipmentDetail();
                detail.OrderNbr
                    = salesOrderRecord.AcumaticaSalesOrderId.ToValue();

                detail.OrderType = "SO".ToValue();
                detail.InventoryID = stockItemId.ToValue();
                detail.WarehouseID = warehouse.AcumaticaWarehouseId.ToValue();

                shipment.Details.Add(detail);
            }

            var resultJson
                = _shipmentClient.AddShipment(shipment.SerializeToJson());

            var resultShipment
                = resultJson.DeserializeFromJson<Shipment>();

            var acumaticaRecord
                = _acumaticaShipmentPull.UpsertShipmentToPersist(resultShipment);

            _syncOrderRepository.InsertShipmentSync(fulfillmentRecord, acumaticaRecord);
        }
    }
}

