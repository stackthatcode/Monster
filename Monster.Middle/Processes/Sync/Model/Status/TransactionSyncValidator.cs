using Monster.Middle.Persist.Instance;
using Monster.Middle.Processes.Shopify.Persist;
using Monster.Middle.Processes.Sync.Model.Orders;
using Push.Foundation.Utilities.Validation;


namespace Monster.Middle.Processes.Sync.Model.Status
{
    public class TransactionSyncValidator
    {
        public ShopifyTransaction ThisTransaction { get; set; }
        public bool ValidPaymentGateway { get; set; }
        public ShopifyOrder ShopifyOrder { get; set; }
        public AcumaticaSalesOrder AcumaticaSalesOrder { get; set; }
        public ShopifyTransaction PaymentTransaction => ShopifyOrder.PaymentTransaction();

        private Validation<TransactionSyncValidator> BuildBaseValidation()
        {
            return new Validation<TransactionSyncValidator>()
                .Add(x => ThisTransaction.DoNotIgnore(), $"Transaction is not valid for sync");
        }

        public ValidationResult ReadyToCreatePayment()
        {
            var validation
                = BuildBaseValidation()
                    .Add(x => !x.ThisTransaction.ExistsInAcumatica(), "Payment has already been created in Acumatica")
                    .Add(x => x.ThisTransaction.IsPayment(), $"Transaction is not a capture or sale")
                    .Add(x => x.AcumaticaSalesOrder == null || x.AcumaticaSalesOrder.AcumaticaIsTaxValid, "Acumatica Sales Order Taxes are invalid");

            return validation.Run(this);
        }

        public ValidationResult ReadyToUpdatePayment()
        {
            var validation
                = BuildBaseValidation()
                    .Add(x => x.ThisTransaction.NeedsPaymentPut, "Payment has already been updated in Acumatica")
                    .Add(x => x.ThisTransaction.ExistsInAcumatica(), "Payment has not been synced")
                    .Add(x => x.ThisTransaction.IsPayment(), $"Transaction is not a capture or sale");

            return validation.Run(this);
        }

        public ValidationResult ReadyToCreateRefundPayment()
        {
            var validation
                = BuildBaseValidation()
                    .Add(x => !x.ThisTransaction.ExistsInAcumatica(), "Refund has been synced already")
                    .Add(x => x.PaymentTransaction.ExistsInAcumatica(), $"Payment has not been synced yet")
                    .Add(x => !x.ShopifyOrder.HasUnreleasedTransactions(), "There are unreleased Payments/Refunds")
                    .Add(x => x.ThisTransaction.IsRefund(), $"Transaction Kind is not a refund");

            return validation.Run(this);
        }

        public ValidationResult ReadyToRelease()
        {
            var validation 
                = BuildBaseValidation()
                    .Add(x => x.ThisTransaction.ExistsInAcumatica(), "Transaction has not been synced to Acumatica yet")
                    .Add(x => !x.ThisTransaction.IsReleased(), "This Transaction has been released already");

            return validation.Run(this);
        }
    }
}
