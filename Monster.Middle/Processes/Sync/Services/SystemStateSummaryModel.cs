using Monster.Middle.Persist.Multitenant;

namespace Monster.Middle.Processes.Sync.Services
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
                this.ShopifyConnection == SystemState.Ok
                && this.AcumaticaConnection == SystemState.Ok
                && this.AcumaticaReferenceData == SystemState.Ok
                && this.PreferenceSelections == SystemState.Ok
                && this.WarehouseSync == SystemState.Ok;
                // && this.InventoryPull == SystemState.Ok;
    }
}
