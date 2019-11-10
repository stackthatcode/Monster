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
        public TransactionPendingAction PaymentPendingAction { get; set; }

        public List<TransactionPendingAction> RefundPendingActions { get; set; }

        public List<AdjustmentMemoPendingAction> AdjustmentMemoPendingActions { get; set; }

        public OrderPendingActionStatus()
        {
            ShopifyOrderAction = PendingAction.None;
            OrderSyncValidation = new ValidationResult();

            MissingShopifyPayment = true;
            PaymentPendingAction = null;

            RefundPendingActions = new List<TransactionPendingAction>();
            AdjustmentMemoPendingActions = new List<AdjustmentMemoPendingAction>();
        }
    }

    public class TransactionPendingAction
    {
        public string TransDesc { get; set; }
        public decimal Amount { get; set; }
        public string PaymentGateway { get; set; }
        public PendingAction Action { get; set; }
        public string ActionDesc => Action.Description();

        public TransactionPendingAction()
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
