using System.Collections.Generic;
using Monster.Acumatica.Api.Common;

namespace Monster.Acumatica.Api.Reference
{
    public class PaymentMethod
    {
        public string id { get; set; }
        public int rowNumber { get; set; }
        public string note { get; set; }

        public BoolValue Active { get; set; }
        public List<AllowedCashAccount> AllowedCashAccounts { get; set; }
        public DateValue CreatedDateTime { get; set; }
        public StringValue Description { get; set; }
        public BoolValue IntegratedProcessing { get; set; }
        public DateValue LastModifiedDateTime { get; set; }
        public StringValue MeansOfPayment { get; set; }
        public StringValue PaymentMethodID { get; set; }
        public BoolValue RequireRemittanceInformationforCashAccount { get; set; }
        public BoolValue UseInAP { get; set; }
        public BoolValue UseInAR { get; set; }
        public StringValue custom { get; set; }
        public List<object> files { get; set; }
    }

}
