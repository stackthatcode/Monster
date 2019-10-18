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
        public const int ImportIntoAcumatica = 6;
        public const int EndToEndSync = 7;


        public static bool IsRecurring(this int jobType)
        {
            return jobType == EndToEndSync;
        }

        public static bool IsOneTime(this int jobType)
        {
            return !jobType.IsRecurring();
        }

        public static readonly Dictionary<int, string> Name = new Dictionary<int, string>()
        {
            { ConnectToAcumatica, "Connect to Acumatica" },
            { RefreshAcumaticaRefData, "Refresh Acumatica Reference Data" },
            { SyncWarehouseAndLocation, "Sync Warehouse and Location" },
            { Diagnostics, "Diagnostics" },
            { RefreshInventory, "RefreshInventory" },
            { ImportIntoAcumatica, "Import into Acumatica" },
            { EndToEndSync, "End To End Sync" },
        };
    }
}
