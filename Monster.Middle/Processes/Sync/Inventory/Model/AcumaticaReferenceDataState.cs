using System.Collections.Generic;
using Monster.Middle.Hangfire;

namespace Monster.Middle.Processes.Sync.Inventory.Model
{
    public class AcumaticaReferenceDataState
    {
        public bool IsRandomAccessMode { get; set; }
        public bool IsBackgroundJobRunning { get; set; }
        public int ReferenceDataState { get; set; }
        public bool IsBroken => ReferenceDataState.IsBroken();
        public List<string> Validations { get; set; }
    }
}
