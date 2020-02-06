using System.Linq;
using Monster.Acumatica.Api.Shipment;
using Monster.Middle.Persist.Instance;
using Monster.Middle.Processes.Shopify.Persist;
using Monster.Middle.Processes.Sync.Misc;
using Monster.Middle.Processes.Sync.Model.Inventory;
using Monster.Middle.Processes.Sync.Model.Orders;
using Monster.Middle.Processes.Sync.Model.PendingActions;
using Monster.Middle.Processes.Sync.Persist;
using Push.Foundation.Utilities.Json;
using Push.Foundation.Utilities.Validation;


namespace Monster.Middle.Processes.Sync.Services
{
    public class FulfillmentStatusService
    {
        private readonly SyncInventoryRepository _syncInventoryRepository;
        private readonly SyncOrderRepository _syncOrderRepository;

        public FulfillmentStatusService(
                SyncInventoryRepository syncInventoryRepository, SyncOrderRepository syncOrderRepository)
        {
            _syncInventoryRepository = syncInventoryRepository;
            _syncOrderRepository = syncOrderRepository;
        }


        public void Validate(ShopifyOrder orderRecord, ShipmentAction action)
        {
            var soShipment = orderRecord.SoShipments()
                .First(x => x.AcumaticaShipmentNbr == action.ShipmentNbr &&
                            x.AcumaticaInvoiceNbr == action.InvoiceNbr);

            action.Validation = Validate(soShipment);
        }

        public ValidationResult Validate(AcumaticaSoShipment shipmentRecord)
        {
            var output = new CreateFulfillmentValidation();

            var salesOrder = shipmentRecord.AcumaticaSalesOrder;
            

            var shopifyOrderId = salesOrder.OriginalShopifyOrder().ShopifyOrderId;

            // Fulfilled in Shopify - thus corrupted!
            //
            output.AnyShopifyMadeFulfillments = _syncOrderRepository.AnyUnsyncedFulfillments(shopifyOrderId);

            // Unmatched Warehouse
            //
            var shipment = shipmentRecord.AcumaticaShipmentJson.DeserializeFromJson<Shipment>();

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
            output.ErrorThresholdExceeded = salesOrder.OriginalShopifyOrder().ExceedsErrorLimit();
            return output.Result();
        }
    }
}

