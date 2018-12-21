namespace Monster.Web.Models.Config
{
    public class AcumaticaStateModel
    {
        public int ConnectionState { get; set; }
        public bool IsUrlFinalized { get; set; }
        public bool IsRandomAccessMode { get; set; }
        public bool IsBackgroundJobRunning { get; set; }
    }
}

