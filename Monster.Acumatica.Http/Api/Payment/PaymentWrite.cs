﻿using Monster.Acumatica.Api.Common;

namespace Monster.Acumatica.Api.Payment
{
    public class PaymentWrite
    {
        public StringValue ReferenceNbr { get; set; }
        public StringValue Type { get; set; }
        public StringValue CustomerID { get; set; }
        public StringValue PaymentMethod { get; set; }
        public StringValue CashAccount { get; set; }
        public StringValue PaymentRef { get; set; }
        public StringValue Description { get; set; }
        public DoubleValue PaymentAmount { get; set; }
    }
}

