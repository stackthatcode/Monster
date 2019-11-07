using Monster.Middle.Persist.Instance;
using Monster.Middle.Processes.Shopify.Persist;
using Monster.Middle.Processes.Sync.Model.Orders;
using Push.Foundation.Utilities.Validation;


namespace Monster.Middle.Processes.Sync.Model.Status
{
    public class PaymentSyncStatus
    {
        public ShopifyTransaction ThisTransaction { get; set; }
        public ShopifyTransaction PaymentTransaction { get; set; }


        public static PaymentSyncStatus Make(ShopifyOrder shopifyOrder, ShopifyTransaction thisTransaction)
        {
            return new PaymentSyncStatus
            {
                ThisTransaction = thisTransaction,
                PaymentTransaction = shopifyOrder.PaymentTransaction(),
            };
        }

        private Validation<PaymentSyncStatus> BuildBaseValidation()
        {
            return new Validation<PaymentSyncStatus>()
                .Add(x => ThisTransaction.DoNotIgnore(), $"Transaction is not valid for sync");
        }

        public ValidationResult MustCreatePayment()
        {
            var validation
                = BuildBaseValidation()
                    .Add(x => !ThisTransaction.ExistsInAcumatica(), "Payment has already been created in Acumatica")
                    .Add(x => ThisTransaction.IsPayment(), $"Transaction is not a capture or sale");

            return validation.Run(this);
        }

        public ValidationResult MustUpdatePayment()
        {
            var validation
                = BuildBaseValidation()
                    .Add(x => ThisTransaction.ExistsInAcumatica(), "Payment has not been synced")
                    .Add(x => x.ThisTransaction.NeedsPaymentPut, "Payment has already been updated in Acumatica")
                    .Add(x => ThisTransaction.IsPayment(), $"Transaction is not a capture or sale");

            return validation.Run(this);
        }

        public ValidationResult MustCreateRefundPayment()
        {
            var validation
                = BuildBaseValidation()
                    .Add(x => PaymentTransaction.ExistsInAcumatica(), $"Payment has not been synced yet")
                    .Add(x => PaymentTransaction.IsReleased(), $"Payment has not been released yet")
                    .Add(x => !ThisTransaction.ExistsInAcumatica(), "Refund has been synced already")
                    .Add(x => ThisTransaction.IsRefund(), $"Transaction Kind is not a refund");

            return validation.Run(this);
        }

        public ValidationResult MustRelease()
        {
            var validation 
                = BuildBaseValidation()
                    .Add(x => x.ThisTransaction.ExistsInAcumatica(), "Transaction has not been synced to Acumatica yet")
                    .Add(x => !x.ThisTransaction.IsReleased(), "This Transaction has been released already");

            return validation.Run(this);
        }
    }
}
