using Monster.Middle.Processes.Sync.Inventory.Model;

namespace Monster.Web.Models.Config
{
    public class WarehouseSyncStatusModel
    {
        public bool IsRandomAccessMode { get; set; }
        public bool IsJobRunning { get; set; }
        public int WarehouseSyncState { get; set; }
        public WarehouseSyncStateDetails Details { get; set; }
    }
}

