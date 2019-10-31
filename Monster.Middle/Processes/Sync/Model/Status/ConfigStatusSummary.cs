using Monster.Middle.Misc.State;
using Monster.Middle.Processes.Sync.Model.Settings;

namespace Monster.Middle.Processes.Sync.Model.Status
{
    public class ConfigStatusSummary
    {
        public int ShopifyConnection { get; set; }
        public int AcumaticaConnection { get; set; }
        public int AcumaticaReferenceData { get; set; }
        public int Settings { get; set; }
        public int SettingsTax { get; set; }
        public int WarehouseSync { get; set; }
        public int StartingShopifyOrder { get; set; }

        // BUT => why not include Inventory Refresh State...?

        public bool IsConfigReadyForEndToEnd =>
            this.ShopifyConnection == StateCode.Ok
            && this.AcumaticaConnection == StateCode.Ok
            && this.AcumaticaReferenceData == StateCode.Ok
            && this.Settings == StateCode.Ok
            && this.SettingsTax == StateCode.Ok
            && this.WarehouseSync == StateCode.Ok;

        // Starting Shopify Order
        //
        public bool IsStartingOrderReadyForEndToEnd => StartingShopifyOrder == StateCode.Ok;

        public bool CanEndToEndSyncBeStarted 
                => IsConfigReadyForEndToEnd && IsStartingOrderReadyForEndToEnd;
    }
}
