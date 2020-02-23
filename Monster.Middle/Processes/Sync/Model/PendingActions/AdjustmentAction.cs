using Push.Foundation.Utilities.Validation;
using Push.Shopify.Api.Order;


namespace Monster.Middle.Processes.Sync.Model.PendingActions
{
    public class AdjustmentAction
    {
        public long ShopifyOrderId { get; set; }
        public long ShopifyRefundId { get; set; }

        public AdjustmentMemoType MemoType { get; set; }
        public string MemoTypeDesc => MemoType.ToString();
        public decimal MemoAmount { get; set; }

        public ActionCode ActionCode { get; set; }
        public string ActionDesc => ActionCode.Description();
        public ValidationResult Validation { get; set; }
        public bool IsValid => Validation.Success;
        public bool IsManualApply => ActionCode == ActionCode.NeedManualApply;

        public string AcumaticaRefNbr { get; set; }
        public string AcumaticaDocType { get; set; }
        public string AcumaticaHref { get; set; }


        public AdjustmentAction()
        {
            ActionCode = ActionCode.None;
        }
    }

}
