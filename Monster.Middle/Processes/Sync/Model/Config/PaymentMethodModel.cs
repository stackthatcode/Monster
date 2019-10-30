using System.Collections.Generic;
using System.Linq;
using Monster.Acumatica.Api.Reference;

namespace Monster.Middle.Processes.Sync.Model.Config
{
    public class PaymentMethodModel
    {
        public string PaymentMethod { get; set; }
        public List<string> CashAccounts { get; set; }

        public PaymentMethodModel(AcumaticaPaymentMethod input)
        {
            PaymentMethod = input.PaymentMethodID.value;

            CashAccounts 
                = input
                    .AllowedCashAccounts
                    .Select(x => x.CashAccount.value)
                    .ToList();
        }
    }
}
