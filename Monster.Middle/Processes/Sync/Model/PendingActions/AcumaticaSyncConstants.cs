namespace Monster.Middle.Processes.Sync.Model.Analysis
{
    public class AcumaticaSyncConstants
    {
        // Used for Payments that exist in Acumatica, but for which Acumatica never returned information
        //
        public const string UnknownRefNbr = "UNKNOWN";

        // Used for Sales Order that are never created in Acumatica
        //
        public const string BlankRefNbr = "BLANK";
    }
}
