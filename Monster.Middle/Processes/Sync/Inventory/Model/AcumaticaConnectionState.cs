namespace Monster.Middle.Processes.Sync.Inventory.Model
{
    public class AcumaticaConnectionState
    {
        public int ConnectionState { get; set; }
        public bool IsUrlFinalized { get; set; }
        public bool IsRandomAccessMode { get; set; }
        public bool IsBackgroundJobRunning { get; set; }
    }
}

