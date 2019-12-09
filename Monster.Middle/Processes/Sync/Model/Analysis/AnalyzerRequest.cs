using Push.Foundation.Utilities.General;

namespace Monster.Middle.Processes.Sync.Model.Analysis
{
    public class AnalyzerRequest
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string SearchText { get; set; }
        public string OrderStatus { get; set; }
        public string SyncFilter { get; set; }

        public int StartRecord => PagingHelper.StartingRecord(PageNumber, PageSize);
    }

    public class AnalyzerStatus
    {
        public const string All = "All";
        public const string Blocked = "Blocked";
        public const string Errors = "Errors";
    }
}
