using System.Collections.Generic;
using Monster.Middle.Processes.Sync.Model.Config;

namespace Monster.Web.Models.Config
{
    public class WarehouseSyncStatusModel
    {
        public bool IsRandomAccessMode { get; set; }
        public bool IsJobRunning { get; set; }
        public List<ExecutionLogModel> ExecutionLogs { get; set; }

        public int WarehouseSyncState { get; set; }
        public WarehouseSyncStateDetails Details { get; set; }
    }
}

