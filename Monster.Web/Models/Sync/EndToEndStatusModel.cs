namespace Monster.Web.Models.Sync
{
    public class EndToEndStatusModel
    {
        public bool AreAnyJobsRunning { get; set; }
        public NonRunningStateModel NonRunningStateModel { get; set; }
        public RunningStateModel RunningStateModel { get; set; }
    }
}