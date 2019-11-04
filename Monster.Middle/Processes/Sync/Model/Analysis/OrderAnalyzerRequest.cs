namespace Monster.Middle.Processes.Sync.Model.Analysis
{
    public class OrderAnalyzerRequest
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string SearchText { get; set; }
    }
}
