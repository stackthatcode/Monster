using System.Collections.Generic;
using System.Linq;
using Monster.Acumatica.Api.Common;

namespace Monster.Acumatica.Api.Payment
{
    public class PaymentWrite
    {
        public StringValue ReferenceNbr { get; set; }
        public BoolValue Hold { get; set; }
        public StringValue Type { get; set; }
        public StringValue CustomerID { get; set; }
        public StringValue PaymentMethod { get; set; }
        public StringValue CashAccount { get; set; }
        public StringValue PaymentRef { get; set; }
        public StringValue Description { get; set; }
        public DoubleValue PaymentAmount { get; set; }
        public DoubleValue AppliedToDocuments { get; set; }

        public List<PaymentOrdersRef> OrdersToApply { get; set; }
        public List<PaymentDocumentsToApply> DocumentsToApply { get; set; }

        public double AmountAppliedToOrder =>
            OrdersToApply == null ? 0d : OrdersToApply.Sum(x => x.AppliedToOrder.value);
    }
}

