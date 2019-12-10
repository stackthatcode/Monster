using System.Collections.Generic;

namespace Monster.Middle.Processes.Sync.Model.PendingActions
{
    public class RootAction
    {
        public string ShopifyOrderName { get; set; }
        public long ShopifyOrderId { get; set; }
        public string ShopifyOrderHref { get; set; }

        // Order sync actions
        //
        public OrderAction OrderAction { get; set; }

        // Payment sync actions
        //
        public PaymentAction PaymentAction { get; set; }
        public List<PaymentAction> RefundPaymentActions { get; set; }
        public List<AdjustmentAction> AdjustmentMemoActions { get; set; }
        public List<ShipmentAction> ShipmentInvoiceActions { get; set; }

        public RootAction()
        {
            PaymentAction = new PaymentAction();
            RefundPaymentActions = new List<PaymentAction>();
            AdjustmentMemoActions = new List<AdjustmentAction>();
            ShipmentInvoiceActions = new List<ShipmentAction>();
        }
    }
}
