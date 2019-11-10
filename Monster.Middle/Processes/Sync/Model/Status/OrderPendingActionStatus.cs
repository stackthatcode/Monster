using System.Collections.Generic;
using Push.Foundation.Utilities.Validation;
using Push.Shopify.Api.Order;

namespace Monster.Middle.Processes.Sync.Model.Status
{
    public class OrderPendingActionStatus
    {
        public string ShopifyOrderName { get; set; }
        public long ShopifyOrderId { get; set; }
        public string ShopifyOrderHref { get; set; }

        public PendingAction ShopifyOrderAction { get; set; }
        public string ShopifyOrderActionDesc => ShopifyOrderAction.Description();
        public ValidationResult OrderSyncValidation { get; set; }

        public bool MissingShopifyPayment { get; set; }
        public decimal ShopifyPaymentAmount { get; set; }
        public PendingAction ShopifyPaymentAction { get; set; }
        public string ShopifyPaymentActionDesc => ShopifyPaymentAction.Description();

        public List<RefundPendingAction> RefundPendingActions { get; set; }

        public List<AdjustmentMemoPendingAction> AdjustmentMemoPendingActions { get; set; }

        public OrderPendingActionStatus()
        {
            ShopifyOrderAction = PendingAction.None;
            ShopifyPaymentAction = PendingAction.None;
            RefundPendingActions = new List<RefundPendingAction>();
            AdjustmentMemoPendingActions = new List<AdjustmentMemoPendingAction>();
        }
    }

    public class RefundPendingAction
    {
        public decimal RefundAmount { get; set; }
        public PendingAction Action { get; set; }
        public string ActionDesc => Action.Description();

        public RefundPendingAction()
        {
            Action = PendingAction.None;
        }
    }

    public class AdjustmentMemoPendingAction
    {
        public AdjustmentMemoType MemoType { get; set; }
        public string MemoTypeDesc => MemoType.ToString();

        
        public decimal MemoAmount { get; set; }
        public PendingAction Action { get; set; }
        public string ActionDesc => Action.Description();

        public AdjustmentMemoPendingAction()
        {
            Action = PendingAction.None;
        }
    }
}
