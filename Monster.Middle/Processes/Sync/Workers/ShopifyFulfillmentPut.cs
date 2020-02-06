using System;
using System.Collections.Generic;
using Monster.Acumatica.Api.Shipment;
using Monster.Middle.Misc.Hangfire;
using Monster.Middle.Misc.Logging;
using Monster.Middle.Persist.Instance;
using Monster.Middle.Processes.Acumatica.Persist;
using Monster.Middle.Processes.Shopify.Persist;
using Monster.Middle.Processes.Sync.Misc;
using Monster.Middle.Processes.Sync.Model.Inventory;
using Monster.Middle.Processes.Sync.Model.Orders;
using Monster.Middle.Processes.Sync.Persist;
using Monster.Middle.Processes.Sync.Services;
using Push.Foundation.Utilities.Json;
using Push.Foundation.Utilities.Logging;
using Push.Shopify.Api;
using Push.Shopify.Api.Inventory;
using Push.Shopify.Api.Order;

namespace Monster.Middle.Processes.Sync.Workers
{
    public class ShopifyFulfillmentPut
    {
        private readonly FulfillmentApi _fulfillmentApi;
        private readonly ShopifyOrderRepository _shopifyOrderRepository;
        private readonly SyncOrderRepository _syncOrderRepository;
        private readonly SyncInventoryRepository _syncInventoryRepository;
        private readonly ExecutionLogService _logService;
        private readonly FulfillmentStatusService _fulfillmentStatusService;
        private readonly JobMonitoringService _jobMonitoringService;
        private readonly IPushLogger _pushLogger;

        public ShopifyFulfillmentPut(
                ShopifyOrderRepository shopifyOrderRepository,
                SyncOrderRepository syncOrderRepository,
                SyncInventoryRepository syncInventoryRepository,
                FulfillmentApi fulfillmentApi, 
                ExecutionLogService logService, 
                FulfillmentStatusService fulfillmentStatusService, 
                IPushLogger pushLogger, JobMonitoringService jobMonitoringService)
        {
            _shopifyOrderRepository = shopifyOrderRepository;
            _syncOrderRepository = syncOrderRepository;
            _syncInventoryRepository = syncInventoryRepository;
            _fulfillmentApi = fulfillmentApi;
            _logService = logService;
            _fulfillmentStatusService = fulfillmentStatusService;
            _pushLogger = pushLogger;
            _jobMonitoringService = jobMonitoringService;
        }


        public void Run()
        {
            // return;
            // SKOUTS HONOR

            var salesOrderRefs = _syncOrderRepository.RetrieveUnsyncedSoShipments();

            foreach (var salesOrderRef in salesOrderRefs)
            {
                if (_jobMonitoringService.DetectCurrentJobInterrupt())
                {
                    return;
                }

                var syncReadiness = _fulfillmentStatusService.Validate(salesOrderRef);

                if (syncReadiness.Success)
                {
                    PushFulfillmentToShopifyAux(salesOrderRef);
                }
            }
        }

        private void PushFulfillmentToShopifyAux(AcumaticaSoShipment salesOrderShipment)
        {
            try
            {
                PushFulfillmentToShopify(salesOrderShipment);
            }
            catch (Exception ex)
            {
                _pushLogger.Error(ex);
                _logService.Log($"Encounter error syncing {salesOrderShipment.LogDescriptor()}");
                _syncOrderRepository.IncreaseOrderErrorCount(
                        salesOrderShipment.AcumaticaSalesOrder.ShopifyOrder.ShopifyOrderId);
                throw;
            }
        }

        private void PushFulfillmentToShopify(AcumaticaSoShipment salesOrderShipment)
        {
            var salesOrderNbr = salesOrderShipment.AcumaticaSalesOrder.AcumaticaOrderNbr;
            var orderRecord = _syncOrderRepository.RetrieveSalesOrder(salesOrderNbr);

            var shopifyOrderRecord = orderRecord.OriginalShopifyOrder();
            var shopifyOrder = shopifyOrderRecord.ToShopifyObj();            
            var shipment = salesOrderShipment.AcumaticaShipmentJson.DeserializeFromJson<Shipment>();
            
            var location = RetrieveMatchingLocation(shipment);

            // Line Items
            //
            var details = shipment.DetailByOrder(orderRecord.AcumaticaOrderNbr);
            var fulfillment = new Fulfillment();
            fulfillment.line_items = new List<LineItem>();
            fulfillment.location_id = location.id;
            fulfillment.tracking_number = salesOrderShipment.AcumaticaTrackingNbr;

            // Build the Detail
            //
            foreach (var detail in details)
            {
                var stockItem = _syncInventoryRepository.RetrieveStockItem(detail.InventoryID.value);
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

            // Write Execution Log entry
            //
            var content = LogBuilder.CreateShopifyFulfillment(salesOrderShipment);
            _logService.Log(content);

            // Write the Fulfillment to the Shopify API
            //
            var parent = new FulfillmentParent() { fulfillment = fulfillment };
            var result = _fulfillmentApi.Insert(shopifyOrderRecord.ShopifyOrderId, parent.SerializeToJson());
            var resultFulfillmentParent = result.DeserializeFromJson<FulfillmentParent>();
            
            // Save the result
            //
            var fulfillmentRecord = new ShopifyFulfillment();
            fulfillmentRecord.ShopifyOrderMonsterId = shopifyOrderRecord.MonsterId;
            fulfillmentRecord.ShopifyOrderId = shopifyOrder.id;
            fulfillmentRecord.ShopifyFulfillmentId = resultFulfillmentParent.fulfillment.id;
            fulfillmentRecord.ShopifyTrackingNumber = salesOrderShipment.AcumaticaTrackingNbr;

            fulfillmentRecord.ShopifyStatus = resultFulfillmentParent.fulfillment.status;
            fulfillmentRecord.DateCreated = DateTime.UtcNow;
            fulfillmentRecord.LastUpdated = DateTime.UtcNow;

            using (var transaction = _syncInventoryRepository.BeginTransaction())
            {
                // Create fulfillment record
                //
                _shopifyOrderRepository.InsertFulfillment(fulfillmentRecord);

                // ... and assign Shipment thereto
                //
                salesOrderShipment.ShopifyFulfillment = fulfillmentRecord;
                salesOrderShipment.LastUpdated = DateTime.UtcNow;
                _shopifyOrderRepository.SaveChanges(); 
                transaction.Commit();
            }
        }

        private Location RetrieveMatchingLocation(Shipment shipment)
        {
            var warehouse = _syncInventoryRepository.RetrieveWarehouse(shipment.WarehouseID.value);
            var locationRecord = warehouse.MatchedLocation();
            var location = locationRecord.ShopifyJson.DeserializeFromJson<Location>();
            return location;
        }
    }
}
