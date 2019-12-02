using System;

namespace Monster.Web.Models.Sync
{
    public class OrderSyncSettingsModel
    {
        public long? ShopifyOrderId { get; set; }

        public string ShopifyOrderName { get; set; }

        public DateTime ShopifyOrderCreatedAtUtc { get; set; }

        public int MaxParallelAcumaticaSyncs { get; set; }
        public int MaxNumberOfOrders { get; set; }
    }
}
