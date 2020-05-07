using System;
using System.Collections.Generic;
using System.Linq;
using Monster.Acumatica.Api.Shipment;
using Monster.Middle.Config;
using Monster.Middle.Misc.Hangfire;
using Monster.Middle.Misc.Logging;
using Monster.Middle.Persist.Instance;
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
        private readonly FulfillmentStatusService _fulfillmentStatusService;
        private readonly JobMonitoringService _jobMonitoringService;
        private readonly ShopifyJsonService _shopifyJsonService;
        private readonly ExecutionLogService _logService;
        private readonly IPushLogger _pushLogger;

        public ShopifyFulfillmentPut(
                ShopifyOrderRepository shopifyOrderRepository,
                SyncOrderRepository syncOrderRepository,
                SyncInventoryRepository syncInventoryRepository,
                FulfillmentApi fulfillmentApi, 
                ExecutionLogService logService, 
                FulfillmentStatusService fulfillmentStatusService, 
                IPushLogger pushLogger, JobMonitoringService jobMonitoringService, 
                ShopifyJsonService shopifyJsonService)
        {
            _shopifyOrderRepository = shopifyOrderRepository;
            _syncOrderRepository = syncOrderRepository;
            _syncInventoryRepository = syncInventoryRepository;
            _fulfillmentApi = fulfillmentApi;
            _logService = logService;
            _fulfillmentStatusService = fulfillmentStatusService;
            _pushLogger = pushLogger;
            _jobMonitoringService = jobMonitoringService;
            _shopifyJsonService = shopifyJsonService;
        }


        public void Run()
        {
            if (MonsterConfig.Settings.DisableShopifyPut)
            {
                _logService.Log(LogBuilder.ShopifyPutDisabled());
                return;
            }

            var salesOrderRefs = _syncOrderRepository.RetrieveUnsyncedSoShipments();

            RunWorker(salesOrderRefs);
        }

        public void Run(long shopifyOrderId)
        {
            if (MonsterConfig.Settings.DisableShopifyPut)
            {
                _logService.Log(LogBuilder.ShopifyPutDisabled());
                return;
            }

            var salesOrderRefs = _syncOrderRepository.RetrieveUnsyncedSoShipments(shopifyOrderId);
            RunWorker(salesOrderRefs);
        }


        private void RunWorker(List<AcumaticaSoShipment> soShipmentRefs)
        {
            foreach (var salesOrderRef in soShipmentRefs)
            {
                if (_jobMonitoringService.DetectCurrentJobInterrupt())
                {
                    return;
                }

                PushFulfillmentToShopifyAux(salesOrderRef);
            }
        }


        private void PushFulfillmentToShopifyAux(AcumaticaSoShipment salesOrderShipment)
        {
            try
            {
                CorrectFulfillmentWithUnknownRef(salesOrderShipment.AcumaticaShipmentNbr);

                var syncReadiness = _fulfillmentStatusService.Validate(salesOrderShipment);

                if (syncReadiness.Success)
                {
                    PushFulfillmentToShopify(salesOrderShipment.AcumaticaShipmentNbr);
                }
            }
            catch (Exception ex)
            {
                _pushLogger.Error(ex);
                _logService.Log($"Encounter error syncing {salesOrderShipment.LogDescriptor()}");
                _syncOrderRepository
                    .IncreaseOrderErrorCount(salesOrderShipment.AcumaticaSalesOrder.ShopifyOrder.ShopifyOrderId);
                throw;
            }
        }


        private void CorrectFulfillmentWithUnknownRef(string shipmentNbr)
        {
            var salesOrderShipment = _syncOrderRepository.RetrieveSoShipment(shipmentNbr);
            if (!salesOrderShipment.HasSyncWithUnknownNbr())
            {
                return;
            }

            var fulfillmentRecord = salesOrderShipment.ShopifyFulfillment;

            var shopifyOrder = 
                _shopifyJsonService.RetrieveOrder(
                    salesOrderShipment.AcumaticaSalesOrder.ShopifyOrder.ShopifyOrderId);

            var matches =
                shopifyOrder
                    .fulfillments
                    .Where(x => x.tracking_number == salesOrderShipment.AcumaticaTrackingNbr)
                    .ToList();

            if (!matches.Any())
            {
                var content = LogBuilder.ShopifyFulfillmentWithUnknownRefNoMatches(salesOrderShipment);
                _logService.Log(content);
                _syncOrderRepository.SetErrorCountToMaximum(shopifyOrder.id);
                return;
            }

            if (matches.Count() > 1)
            {
                var content = LogBuilder.ShopifyFulfillmentWithUnknownRefTooManyMatches(salesOrderShipment);
                _logService.Log(content);
                _syncOrderRepository.SetErrorCountToMaximum(shopifyOrder.id);
                return;
            }

            fulfillmentRecord.Ingest(matches.First());
            fulfillmentRecord.DateCreated = DateTime.UtcNow;
            fulfillmentRecord.LastUpdated = DateTime.UtcNow;

            _logService.Log(LogBuilder.FillingUnknownShopifyFulfillmentRef(salesOrderShipment, fulfillmentRecord));
            _shopifyOrderRepository.SaveChanges();
        }


        private void PushFulfillmentToShopify(string shipmentNbr)
        {
            // Get a fresh copy
            //
            var salesOrderShipment = _syncOrderRepository.RetrieveSoShipment(shipmentNbr);

            if (salesOrderShipment.IsSynced())
            {
                return;
            }
            if (salesOrderShipment.HasSyncWithUnknownNbr())
            {
                return;
            }

            var fulfillmentParent = ShopifyOrderRecord(salesOrderShipment);
                
            // Write Execution Log entry
            //
            var content = LogBuilder.CreateShopifyFulfillment(salesOrderShipment);
            _logService.Log(content);

            // First, create the Sync Record
            //
            var orderRecord = salesOrderShipment.AcumaticaSalesOrder.ShopifyOrder;
            var fulfillmentRecord = new ShopifyFulfillment();
            fulfillmentRecord.ShopifyOrderMonsterId = orderRecord.MonsterId;
            fulfillmentRecord.ShopifyOrderId = orderRecord.ShopifyOrderId;
            fulfillmentRecord.DateCreated = DateTime.UtcNow;
            fulfillmentRecord.LastUpdated = DateTime.UtcNow;

            // ... and assign Shipment thereto
            //
            salesOrderShipment.ShopifyFulfillment = fulfillmentRecord;
            salesOrderShipment.LastUpdated = DateTime.UtcNow;
            _shopifyOrderRepository.InsertFulfillment(fulfillmentRecord);

            // Write the Fulfillment to the Shopify API
            //
            var resultJson = _fulfillmentApi.Insert(orderRecord.ShopifyOrderId, fulfillmentParent.SerializeToJson());

            var resultParent = resultJson.DeserializeFromJson<FulfillmentParent>();
            
            // Ingest and save the result
            //
            fulfillmentRecord.Ingest(resultParent.fulfillment);
            fulfillmentRecord.LastUpdated = DateTime.UtcNow;
            _shopifyOrderRepository.SaveChanges();
        }

        private FulfillmentParent ShopifyOrderRecord(AcumaticaSoShipment salesOrderShipment)
        {
            var salesOrderRecord = salesOrderShipment.AcumaticaSalesOrder;
            var orderRecord = _syncOrderRepository.RetrieveSalesOrder(salesOrderRecord.AcumaticaOrderNbr);
            var shopifyOrderRecord = orderRecord.OriginalShopifyOrder();

            var shopifyOrder = _shopifyJsonService.RetrieveOrder(shopifyOrderRecord.ShopifyOrderId);
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
                    quantity = (int) quantity,
                };

                fulfillment.line_items.Add(fulfillmentDetail);
            }

            var parent = new FulfillmentParent() {fulfillment = fulfillment};
            return parent;
        }

        private Location RetrieveMatchingLocation(Shipment shipment)
        {
            var warehouse = _syncInventoryRepository.RetrieveWarehouse(shipment.WarehouseID.value);
            var locationRecord = warehouse.MatchedLocation();
            var location = 
                _shopifyJsonService
                    .RetrieveJson(ShopifyJsonType.Location, locationRecord.ShopifyLocationId)
                    .DeserializeFromJson<Location>();
            return location;
        }
    }
}

