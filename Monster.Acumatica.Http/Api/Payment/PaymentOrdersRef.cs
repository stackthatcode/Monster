using System.Collections.Generic;
using Monster.Acumatica.Api.Common;

namespace Monster.Acumatica.Api.Payment
{
    public class PaymentOrdersRef
    {
        public StringValue OrderNbr { get; set; }
        public StringValue OrderType { get; set; }
        public DoubleValue AppliedToOrder { get; set; }

        public static List<PaymentOrdersRef> 
                ForOrder(string orderNbr, string orderType, double appliedToOrder)
        {
            return new List<PaymentOrdersRef>()
            {
                new PaymentOrdersRef()
                {
                    OrderNbr = orderNbr.ToValue(),
                    OrderType = orderType.ToValue(),
                    AppliedToOrder = appliedToOrder.ToValue(),
                }
            };
        }
    }
}
