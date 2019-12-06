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
        public string Warehouse { get; set; }
        public string WarehouseID => Warehouse.Trim();

        public DateTime INSiteStatus_lastModifiedDateTime { get; set; }
        public double QtyAvailable { get; set; }
    }
}

