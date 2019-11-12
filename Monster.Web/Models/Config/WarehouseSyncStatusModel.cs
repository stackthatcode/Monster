using Monster.Middle.Processes.Sync.Model.Status;

namespace Monster.Web.Models.Config
{
    public class WarehouseSyncStatusModel
    {
        public bool IsRandomAccessMode { get; set; }
        public int WarehouseSyncState { get; set; }
        public WarehouseSyncSummary Details { get; set; }
    }
}

