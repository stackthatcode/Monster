using System.Collections.Generic;

namespace Monster.Middle.Processes.Sync.Model.PendingActions
{
    public class RootAction
    {
        public OrderAction OrderAction { get; set; }
        public PaymentAction PaymentAction { get; set; }
        public List<PaymentAction> RefundPaymentActions { get; set; }
        public List<AdjustmentAction> AdjustmentMemoActions { get; set; }
        public List<ShipmentAction> ShipmentInvoiceActions { get; set; }
        
        public RootAction()
        {
            OrderAction = new OrderAction();
            PaymentAction = new PaymentAction();
            RefundPaymentActions = new List<PaymentAction>();
            AdjustmentMemoActions = new List<AdjustmentAction>();
            ShipmentInvoiceActions = new List<ShipmentAction>();
        }
    }
}
