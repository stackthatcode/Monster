namespace Monster.Web.Models.Config
{
    public class SystemStateSummaryModel
    {
        public int ShopifyConnection { get; set; } // ShopifyConnection
        public int AcumaticaConnection { get; set; } // AcumaticaConnection
        public int AcumaticaReferenceData { get; set; } // AcumaticaReferenceData
        public int PreferenceSelections { get; set; } // PreferenceSelections
        public int WarehouseSync { get; set; } // WarehouseSync
        public int AcumaticaInventoryPush { get; set; } // AcumaticaInventoryPush
        public int ShopifyInventoryPush { get; set; } // ShopifyInventoryPush
        public bool IsReadyForRealTimeSync { get; set; }
    }
}