namespace Monster.Middle.Processes.Sync.Model.Analysis
{
    public static class Extensions
    {
        public static string AnalysisFormat(this decimal? input)
        {
            return (input ?? 0).ToString("0.00");
        }

        public static string AnalysisFormat(this decimal input)
        {
            return AnalysisFormat((decimal?)input);
        }
    }
}
