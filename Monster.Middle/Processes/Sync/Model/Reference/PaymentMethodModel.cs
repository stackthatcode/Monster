using System.Collections.Generic;
using System.Linq;
using Monster.Acumatica.Api.Reference;
using Push.Foundation.Utilities.Validation;

namespace Monster.Middle.Processes.Sync.Model.Reference
{
    public class PaymentMethodModel
    {
        private AcumaticaPaymentMethod Data { get; set; }

        public string PaymentMethod => Data.PaymentMethodID.value;
        public List<string> CashAccounts => Data.AllowedCashAccounts.Select(x => x.CashAccount.value).ToList();

        public ValidationResult Validation
        {
            get
            {
                return new Validation<PaymentMethodModel>()
                    .Add(x => x.Data.UseInAR.value, "Payment Method does not have 'Use In AR' set")
                    .Add(x => x.Data.AllowedCashAccounts.Any(y => y.UseInAR.value),
                        "Payment Method does not have any Cash Accounts with 'Use In AR' set")
                    .Run(this);
            }
        }

        public PaymentMethodModel(AcumaticaPaymentMethod input)
        {
            Data = input;
        }
    }
}
