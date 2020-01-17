using System.Collections.Generic;
using System.Linq;

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

        public bool HasPendingOrderActions => OrderAction.ActionCode != ActionCode.None;

        public bool HasPendingPaymentActions => PaymentAction.ActionCode != ActionCode.None;

        public bool HasPendingRefundActions => RefundPaymentActions.Any(x => x.ActionCode != ActionCode.None);


        public bool HasPendingActions
        {
            get
            {
                if (HasPendingOrderActions || HasPendingPaymentActions || HasPendingRefundActions)
                {
                    return true;
                }

                if (AdjustmentMemoActions.Any(x => x.ActionCode != ActionCode.None))
                {
                    return true;
                }
                if (ShipmentInvoiceActions.Any(x => x.ActionCode != ActionCode.None))
                {
                    return true;
                }

                return false;
            }
        }
    }
}

