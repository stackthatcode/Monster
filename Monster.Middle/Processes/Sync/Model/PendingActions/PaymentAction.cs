using Monster.Middle.Processes.Sync.Model.Status;
using Push.Foundation.Utilities.Validation;


namespace Monster.Middle.Processes.Sync.Model.PendingActions
{
    public class PaymentAction
    {
        public long ShopifyTransactionId { get; set; }
        public string TransDesc { get; set; }
        public decimal Amount { get; set; }
        public string PaymentGateway { get; set; }
        public ActionCode ActionCode { get; set; }
        public ValidationResult Validation { get; set; }

        // Computed helpers
        //
        public string ActionDesc => ActionCode.Description();
        public bool IsValid => Validation.Success;


        public PaymentAction()
        {
            Validation = new ValidationResult();
            ActionCode = ActionCode.None;
        }
    }
}
