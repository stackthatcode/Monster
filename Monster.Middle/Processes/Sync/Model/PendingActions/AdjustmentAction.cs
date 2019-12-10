using Monster.Middle.Processes.Sync.Model.Status;
using Push.Shopify.Api.Order;


namespace Monster.Middle.Processes.Sync.Model.PendingActions
{
    public class AdjustmentAction
    {
        public AdjustmentMemoType MemoType { get; set; }
        public string MemoTypeDesc => MemoType.ToString();

        public decimal MemoAmount { get; set; }
        public ActionCode ActionCode { get; set; }
        public string ActionDesc => ActionCode.Description();

        public AdjustmentAction()
        {
            ActionCode = ActionCode.None;
        }
    }

}
