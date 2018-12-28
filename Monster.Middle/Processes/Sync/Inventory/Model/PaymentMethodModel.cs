using System.Collections.Generic;
using System.Linq;
using Monster.Acumatica.Api.Reference;

namespace Monster.Middle.Processes.Sync.Inventory.Model
{
    public class PaymentMethodModel
    {
        public string PaymentMethod { get; set; }
        public List<string> CashAccounts { get; set; }

        public PaymentMethodModel(PaymentMethod input)
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
