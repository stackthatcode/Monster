using Monster.Acumatica.Api.Shipment;
using Monster.Middle.Persist.Instance;
using Monster.Middle.Processes.Sync.Misc;
using Monster.Middle.Processes.Sync.Model.Inventory;
using Monster.Middle.Processes.Sync.Model.Orders;
using Monster.Middle.Processes.Sync.Model.PendingActions;
using Monster.Middle.Processes.Sync.Persist;
using Push.Foundation.Utilities.Json;


namespace Monster.Middle.Processes.Sync.Services
{
    public class FulfillmentStatusService
    {
        private readonly SyncInventoryRepository _syncInventoryRepository;
        private readonly SyncOrderRepository _syncOrderRepository;

        public FulfillmentStatusService(
                SyncInventoryRepository syncInventoryRepository, 
                SyncOrderRepository syncOrderRepository)
        {
            _syncInventoryRepository = syncInventoryRepository;
            _syncOrderRepository = syncOrderRepository;
        }


        public CreateFulfillmentValidation ReadyToSync(AcumaticaSoShipment soShipment)
        {
            var output = new CreateFulfillmentValidation();

            var salesOrder = 
                _syncOrderRepository.RetrieveSalesOrder(soShipment.AcumaticaSalesOrder.AcumaticaOrderNbr);
            var shopifyOrderId = salesOrder.OriginalShopifyOrder().ShopifyOrderId;

            // Fulfilled in Shopify - thus corrupted!
            //
            output.AnyShopifyMadeFulfillments = _syncOrderRepository.AnyUnsyncedFulfillments(shopifyOrderId);

            // Unmatched Warehouse
            //
            var shipment = soShipment.AcumaticaShipmentJson.DeserializeFromJson<Shipment>();

            var warehouseRecord = _syncInventoryRepository.RetrieveWarehouse(shipment.WarehouseID.value);

            var locationRecord = warehouseRecord.MatchedLocation();
            if (locationRecord == null)
            {
                output.WarehouseLocationUnmatched = true;
            }

            // Unmatched Stock Item/Inventory
            //
            foreach (var line in shipment.Details)
            {
                var stockItem = _syncInventoryRepository.RetrieveStockItem(line.InventoryID.value);
                var variant = stockItem.MatchedVariant();

                if (variant == null)
                {
                    output.UnmatchedVariantStockItems.Add(stockItem.ItemId);
                    continue;
                }
            }

            // Error threshold
            //
            output.ErrorThresholdExceeded = soShipment.PutErrorCount >= SystemConsts.ErrorThreshold;

            return output;
        }
    }
}

