namespace Monster.Web.Models.Config
{
    public class SyncEnablesModel
    {
        public bool SyncOrdersEnabled { get; set; }
        public bool SyncInventoryEnabled { get; set; }
        public bool SyncShipmentsEnabled { get; set; }
        public bool SyncRefundsEnabled { get; set; }
    }
}

