namespace Monster.Web.Models.Sync
{
    public class SyncSettingsModel
    {
        public bool SyncOrdersEnabled { get; set; }
        public bool SyncInventoryEnabled { get; set; }
        public bool SyncShipmentsEnabled { get; set; }
        public bool SyncRefundsEnabled { get; set; }

        public long? StartingOrderId { get; set; }
        public string StartingOrderName { get; set; }
        public string StartOrderCreatedAtUtc { get; set; }
        public int MaxParallelAcumaticaSyncs { get; set; }
        public int MaxNumberOfOrders { get; set; }

        public string StartingOrderHref { get; set; }
        public bool IsStartingOrderSet => StartingOrderId.HasValue;

        public bool InventorySyncAvailableQty { get; set; }
        public bool InventorySyncPrice { get; set; }
        public bool InventorySyncWeight { get; set; }
    }
}

