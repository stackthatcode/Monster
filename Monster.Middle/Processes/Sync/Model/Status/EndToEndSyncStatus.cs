using Monster.Middle.Misc.State;
using Monster.Middle.Processes.Sync.Model.Settings;

namespace Monster.Middle.Processes.Sync.Model.Status
{
    public class EndToEndSyncStatus
    {
        // Config State
        //
        public ConfigStatusSummaryModel ConfigStatusSummaryModel { get; set; }

        // Starting Shopify Order
        //
        public int ShopifyOrderState { get; set; }
        public bool IsStartingOrderReadyForEndToEnd => ShopifyOrderState == StateCode.Ok;

        public bool CanEndToEndSyncBeStarted 
            => ConfigStatusSummaryModel.IsConfigReadyForEndToEnd && IsStartingOrderReadyForEndToEnd;
    }
}
