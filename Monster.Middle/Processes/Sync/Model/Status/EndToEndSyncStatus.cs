using Monster.Middle.Misc.State;

namespace Monster.Middle.Processes.Sync.Model.Status
{
    public class EndToEndSyncStatus
    {
        // Config State
        //
        public ConfigStateSummaryModel ConfigStateSummaryModel { get; set; }

        // Starting Shopify Order
        //
        public int ShopifyOrderState { get; set; }
        public bool IsStartingOrderReadyForEndToEnd => ShopifyOrderState == StateCode.Ok;

        public bool CanEndToEndSyncBeStarted 
            => ConfigStateSummaryModel.IsConfigReadyForEndToEnd && IsStartingOrderReadyForEndToEnd;
    }
}
