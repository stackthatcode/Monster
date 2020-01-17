namespace Monster.Web.Models.Sync
{
    public class SyncEnablesUpdateModel
    {
        public bool PullFromAcumaticaEnabled { get; set; }
        public bool PullFromShopifyEnabled { get; set; }
        public bool SyncOrdersEnabled { get; set; }
        public bool SyncInventoryEnabled { get; set; }
        public bool SyncShipmentsEnabled { get; set; }
        public bool SyncRefundsEnabled { get; set; }
    }
}