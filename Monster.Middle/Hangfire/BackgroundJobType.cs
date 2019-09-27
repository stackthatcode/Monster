﻿using System.Collections.Generic;

namespace Monster.Middle.Hangfire
{
    public class BackgroundJobType
    {
        public const int ConnectToAcumatica = 1;
        public const int PullAcumaticaRefData = 2;
        public const int SyncWarehouseAndLocation = 3;
        public const int Diagnostics = 4;
        public const int PullInventory = 5;
        public const int ImportIntoAcumatica = 6;
        public const int EndToEndSync = 7;

        public readonly Dictionary<int, string> Name = new Dictionary<int, string>()
        {
            { ConnectToAcumatica, "ConnectToAcumatica" },
            { PullAcumaticaRefData, "PullAcumaticaRefData" },
            { SyncWarehouseAndLocation, "SyncWarehouseAndLocation" },
            { Diagnostics, "Diagnostics" },
            { PullInventory, "PullInventory" },
            { ImportIntoAcumatica, "ImportIntoAcumatica" },
            { EndToEndSync, "EndToEndSync" },
        };
    }
}
