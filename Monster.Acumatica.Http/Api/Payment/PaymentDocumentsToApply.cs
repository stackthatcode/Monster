using System.Collections.Generic;
using Monster.Acumatica.Api.Common;

namespace Monster.Acumatica.Api.Payment
{
    public class PaymentDocumentsToApply
    {
        public StringValue ReferenceNbr { get; set; }
        public StringValue DocType { get; set; }
        public DoubleValue AmountPaid { get; set; }


        public static List<PaymentDocumentsToApply> 
                ForDocument(string referenceNbr, string docType, double amountPaid)
        {
            return new List<PaymentDocumentsToApply>()
            {
                new PaymentDocumentsToApply()
                {
                    ReferenceNbr = referenceNbr.ToValue(),
                    DocType = docType.ToValue(),
                    AmountPaid = amountPaid.ToValue(),
                }
            };
        }
    }
}

