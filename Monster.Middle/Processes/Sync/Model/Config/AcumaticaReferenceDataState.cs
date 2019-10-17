using System.Collections.Generic;
using Monster.Middle.Misc.State;

namespace Monster.Middle.Processes.Sync.Model.Config
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
