using System.Collections.Generic;
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
        public OrderSyncValidation OrderSyncValidation { get; set; }

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

        public RefundPendingAction()
        {
            Action = PendingAction.None;
        }
    }

    public class AdjustmentMemoPendingAction
    {
        public AdjustmentMemoType MemoType { get; set; }
        public decimal MemoAmount { get; set; }
        public PendingAction Action { get; set; }

        public AdjustmentMemoPendingAction()
        {
            Action = PendingAction.None;
        }
    }
}
