using System.Linq;
using Monster.Acumatica.Api.SalesOrder;
using Monster.Middle.Persist.Instance;
using Push.Shopify.Api.Order;

namespace Monster.Middle.Processes.Sync.Model.Orders
{
    public static class ShipmentExtensions
    {

        public static bool HasSyncWithUnknownNbr(this AcumaticaSoShipment shipment)
        {
            return shipment.ShopifyFulfillment != null &&
                   shipment.ShopifyFulfillment.ShopifyFulfillmentId == null;
        }

        public static bool IsSynced(this AcumaticaSoShipment shipment)
        {
            return shipment.ShopifyFulfillment != null &&
                   shipment.ShopifyFulfillment.ShopifyFulfillmentId.HasValue;
        }

        public static void Ingest(this ShopifyFulfillment record, Fulfillment fulfillment)
        {
            record.ShopifyFulfillmentId = fulfillment.id;
            record.ShopifyTrackingNumber = fulfillment.tracking_number;
            record.ShopifyStatus = fulfillment.status;
        }
    }
}
