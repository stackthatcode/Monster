using Monster.Middle.Processes.Sync.Inventory.Model;

namespace Monster.Web.Models.Config
{
    public class WarehouseSyncStatusModel
    {
        public int JobStatus { get; set; }
        public WarehouseSyncState SyncState { get; set; }        
    }
}

