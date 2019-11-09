using System.Collections.Generic;
using Push.Shopify.Api.Order;

namespace Monster.Middle.Processes.Sync.Model.Status
{
    public class OrderPendingActionStatus
    {
        public string ShopifyOrderName { get; set; }
        public long ShopifyOrderId { get; set; }
        public string ShopifyOrderHref { get; set; }

        public bool CreateOrderInAcumatica { get; set; }
        public bool UpdateOrderInAcumatica { get; set; }
        public OrderSyncValidation OrderSyncValidation { get; set; }

        public bool MissingShopifyPayment { get; set; }
        public decimal ShopifyPaymentAmount { get; set; }
        public bool CreatePaymentInAcumatica { get; set; }
        public bool UpdatePaymentInAcumatica { get; set; }
        public bool ReleasePaymentInAcumatica { get; set; }

        public List<RefundPendingAction> RefundPendingActions { get; set; }

        public List<AdjustmentMemoPendingAction> AdjustmentMemoPendingActions { get; set; }
    }

    public class RefundPendingAction
    {
        public decimal RefundAmount { get; set; }
        public bool CreateCustomerRefundInAcumatica { get; set; }
        public bool ReleaseCustomerRefundInAcumatica { get; set; }
    }

    public class AdjustmentMemoPendingAction
    {
        public AdjustmentMemoType MemoType { get; set; }
        public decimal MemoAmount { get; set; }
    }
}
