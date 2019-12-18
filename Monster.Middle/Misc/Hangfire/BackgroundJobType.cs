using System.Collections.Generic;

namespace Monster.Middle.Misc.Hangfire
{
    public static class BackgroundJobType
    {
        public const int ConnectToAcumatica = 1;
        public const int RefreshAcumaticaRefData = 2;
        public const int SyncWarehouseAndLocation = 3;
        public const int Diagnostics = 4;
        public const int RefreshInventory = 5;
        public const int ImportAcumaticaStockItems = 6;
        public const int SyncWithAcumaticaStockItems = 7;
        public const int ImportNewShopifyProduct = 8;
        public const int ImportAddShopifyVariantsToProduct = 9;
        public const int EndToEndSync = 10;



        public static readonly Dictionary<int, string> Name = new Dictionary<int, string>()
        {
            { ConnectToAcumatica, "Connect to Acumatica" },
            { RefreshAcumaticaRefData, "Refresh Acumatica Reference Data" },
            { SyncWarehouseAndLocation, "Sync Warehouse and Location" },
            { Diagnostics, "Diagnostics" },
            { RefreshInventory, "Refresh Inventory Cache" },
            { ImportAcumaticaStockItems, "Import Stock Items into Acumatica" },
            { SyncWithAcumaticaStockItems,"Sync with Stock Items in Acumatica" },
            { ImportNewShopifyProduct, "Import New Shopify Product" },
            { ImportAddShopifyVariantsToProduct, "Import Add Shopify Variants to Existing Product" },
            { EndToEndSync, "End To End Sync" },
        };
    }
}

