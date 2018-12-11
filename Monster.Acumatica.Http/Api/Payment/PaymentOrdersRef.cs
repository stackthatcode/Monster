using System.Collections.Generic;
using Monster.Acumatica.Api.Common;

namespace Monster.Acumatica.Api.Payment
{
    public class PaymentOrdersRef
    {
        public StringValue OrderNbr { get; set; }

        public static List<PaymentOrdersRef> ForOrder(string orderNbr)
        {
            return new List<PaymentOrdersRef>()
            {
                new PaymentOrdersRef() { OrderNbr = orderNbr.ToValue() }
            };
        }
    }
}
