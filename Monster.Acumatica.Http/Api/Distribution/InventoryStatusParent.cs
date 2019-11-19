using System;
using System.Collections.Generic;

namespace Monster.Acumatica.Api.Distribution
{
    public class InventoryStatusParent
    {
            public List<InventoryStatus> value { get; set; }
    }

    public class InventoryStatus
    {
        public string InventoryID { get; set; }
        public string WarehouseID { get; set; }
        public DateTime INSiteStatus_lastModifiedDateTime { get; set; }
        public double QtyAvailable { get; set; }
    }
}

