using System.Collections.Generic;

namespace Monster.Web.Models.Sync
{
    public class RunningStateModel
    {
        public bool IsEndToEndSyncRunning { get; set; }
        public List<ExecutionLogModel> Logs { get; set; }
    }
}
