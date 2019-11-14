using Push.Foundation.Utilities.General;

namespace Monster.Middle.Processes.Sync.Model.Analysis
{
    public class AnalyzerRequest
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string SearchText { get; set; }

        public int StartRecord => PagingHelper.StartingRecord(PageNumber, PageSize);
    }
}
