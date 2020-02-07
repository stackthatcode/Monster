using System.Collections.Generic;

namespace Monster.Middle.Misc.Hangfire
{
    public static class BackgroundJobType
    {
        public const int ConnectToShopify = 1;
        public const int ConnectToAcumatica = 2;
        public const int RefreshAcumaticaRefData = 3;
        public const int SyncWarehouseAndLocation = 4;
        public const int Diagnostics = 5;
        public const int RefreshInventory = 6;
        public const int ImportAcumaticaStockItems = 7;
        public const int SyncWithAcumaticaStockItems = 8;
        public const int ImportNewShopifyProduct = 9;
        public const int ImportAddShopifyVariantsToProduct = 10;
        public const int EndToEndSync = 11;


        public static readonly Dictionary<int, string> Name = new Dictionary<int, string>()
        {
            { ConnectToShopify, "Connect to Shopify" },
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

