namespace Monster.Web.Models.Sync
{
    public class NonRunningStateModel
    {
        public bool IsConfigReady { get; set; }
        public bool IsStartingOrderReady { get; set; }
        public bool CanStartEndToEnd { get; set; }
    }
}
