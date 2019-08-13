namespace Monster.Middle.Processes.Sync.Model.Misc
{
    public class SystemStateSummaryModel
    {
        public int ShopifyConnection { get; set; }
        public int AcumaticaConnection { get; set; }
        public int AcumaticaReferenceData { get; set; }
        public int PreferenceSelections { get; set; }
        public int WarehouseSync { get; set; }
        public int InventoryPull { get; set; }
        
        public bool IsReadyForRealTimeSync =>
                this.ShopifyConnection == StateCode.Ok
                && this.AcumaticaConnection == StateCode.Ok
                && this.AcumaticaReferenceData == StateCode.Ok
                && this.PreferenceSelections == StateCode.Ok
                && this.WarehouseSync == StateCode.Ok;
                // && this.InventoryPull == StateCode.Ok;
    }
}
