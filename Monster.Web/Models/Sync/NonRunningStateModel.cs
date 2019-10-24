namespace Monster.Web.Models.Sync
{
    public class NonRunningStateModel
    {
        public bool IsConfigReadyForEndToEnd { get; set; }
        public bool IsStartingOrderReadyForEndToEnd { get; set; }
        public bool CanEndToEndSyncBeStarted { get; set; }
    }
}
