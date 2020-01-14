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

        public bool HasPendingActions
        {
            get
            {
                if (OrderAction.ActionCode != ActionCode.None)
                {
                    return true;
                }
                if (PaymentAction.ActionCode != ActionCode.None)
                {
                    return true;
                }
                if (RefundPaymentActions.Any(x => x.ActionCode != ActionCode.None))
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

