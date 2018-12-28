using System.Collections.Generic;

namespace Monster.Middle.Processes.Sync.Inventory.Model
{
    public class AcumaticaReferenceDataState
    {
        public bool IsRandomAccessMode { get; set; }
        public bool IsBackgroundJobRunning { get; set; }
        public int ReferenceDataState { get; set; }
        public List<string> Validations { get; set; }
    }
}
