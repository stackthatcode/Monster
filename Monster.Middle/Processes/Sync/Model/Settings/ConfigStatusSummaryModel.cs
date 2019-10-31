using Monster.Middle.Misc.State;

namespace Monster.Middle.Processes.Sync.Model.Settings
{
    public class ConfigStatusSummaryModel
    {
        public int ShopifyConnection { get; set; }
        public int AcumaticaConnection { get; set; }
        public int AcumaticaReferenceData { get; set; }
        public int Settings { get; set; }
        public int SettingsTax { get; set; }
        public int WarehouseSync { get; set; }
        
        public bool IsConfigReadyForEndToEnd =>
                this.ShopifyConnection == StateCode.Ok
                && this.AcumaticaConnection == StateCode.Ok
                && this.AcumaticaReferenceData == StateCode.Ok
                && this.Settings == StateCode.Ok
                && this.SettingsTax == StateCode.Ok
                && this.WarehouseSync == StateCode.Ok;
    }
}

