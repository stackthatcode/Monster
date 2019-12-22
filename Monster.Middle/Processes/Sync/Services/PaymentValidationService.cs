using Monster.Middle.Persist.Instance;
using Monster.Middle.Processes.Shopify.Persist;
using Monster.Middle.Processes.Sync.Misc;
using Monster.Middle.Processes.Sync.Model.Analysis;
using Monster.Middle.Processes.Sync.Model.Orders;
using Monster.Middle.Processes.Sync.Model.PendingActions;
using Monster.Middle.Processes.Sync.Persist;
using Push.Foundation.Utilities.Validation;

namespace Monster.Middle.Processes.Sync.Services
{
    public class PaymentValidationService
    {
        private readonly SettingsRepository _settingsRepository;

        public PaymentValidationService(SettingsRepository settingsRepository)
        {
            _settingsRepository = settingsRepository;
        }

        private PaymentValidationContext BuildContext(ShopifyTransaction currentTransaction)
        {
            var output = new PaymentValidationContext();
            output.CurrentTransaction = currentTransaction;
            output.ValidPaymentGateway = _settingsRepository.GatewayExistsInConfig(currentTransaction.ShopifyGateway);
            return output;
        }

        private Validation<PaymentValidationContext> BuildBaseValidation()
        {
            return new Validation<PaymentValidationContext>()
                .Add(x => x.CurrentTransaction.DoNotIgnore(), $"Transaction is not valid for synchronization")
                .Add(x => x.CurrentTransaction.PutErrorCount < SystemConsts.ErrorThreshold,
                    "Encountered too many errors attempting to synchronize this Transaction");
        }

        public ValidationResult ReadyToCreatePayment(ShopifyTransaction currentTransaction)
        {
            var context = BuildContext(currentTransaction);

            var validation
                = BuildBaseValidation()
                    .Add(x => !x.CurrentTransaction.ExistsInAcumatica(), "Payment has already been created in Acumatica")
                    .Add(x => x.CurrentTransaction.IsPayment(), $"Transaction is not a capture or sale")
                    .Add(x => x.AcumaticaSalesOrder == null
                              || x.AcumaticaSalesOrder.AcumaticaIsTaxValid,
                            "Acumatica Sales Order Taxes are invalid");

            return validation.Run(context);
        }

        public ValidationResult ReadyToUpdatePayment(ShopifyTransaction currentTransaction)
        {
            var context = BuildContext(currentTransaction);

            var validation
                = BuildBaseValidation()
                    .Add(x => x.CurrentTransaction.NeedsPaymentPut, "Payment has already been updated in Acumatica")
                    .Add(x => x.CurrentTransaction.ExistsInAcumatica(), "Payment has not been synced")
                    .Add(x => x.CurrentTransaction.IsPayment(), $"Transaction is not a capture or sale");

            return validation.Run(context);
        }

        public ValidationResult ReadyToRelease(ShopifyTransaction currentTransaction)
        {
            var context = BuildContext(currentTransaction);

            var validation
                = BuildBaseValidation()
                    .Add(x => x.CurrentTransaction.ExistsInAcumatica(), "Transaction has not been synced to Acumatica yet")
                    .Add(x => !x.CurrentTransaction.IsReleased(), "This Transaction has been released already");

            return validation.Run(context);
        }

        public ValidationResult ReadyToCreateRefundPayment(ShopifyTransaction currentTransaction)
        {
            var context = BuildContext(currentTransaction);

            var validation
                = BuildBaseValidation()
                    .Add(x => !x.CurrentTransaction.ExistsInAcumatica(), "Refund has been synced already")
                    .Add(x => x.OriginalPaymentTransaction.ExistsInAcumatica(), $"Original Payment has not been synced yet")
                    .Add(x => !x.ShopifyOrder.HasUnreleasedTransactions(), "There are unreleased Payments/Refunds")
                    .Add(x => x.CurrentTransaction.IsRefund(), $"Transaction Kind is not a refund");

            return validation.Run(context);
        }

        public ValidationResult ReadyToReleaseRefundPayment(ShopifyTransaction currentTransaction)
        {
            var context = BuildContext(currentTransaction);

            var validation
                = BuildBaseValidation()
                    .Add(x => x.CurrentTransaction.ExistsInAcumatica(), "Refund has not been synced yet")
                    .Add(x => x.OriginalPaymentTransaction.ExistsInAcumatica(), $"Payment has not been synced yet")
                    .Add(x => x.CurrentTransaction.IsRefund(), $"Transaction Kind is not a refund")
                    .Add(x => x.CurrentTransaction.AcumaticaPayment.AcumaticaRefNbr != AcumaticaSyncConstants.UnknownRefNbr,
                            $"Acumatica Payment/Customer Refund has '{AcumaticaSyncConstants.UnknownRefNbr}' Reference Number");

            return validation.Run(context);
        }
    }
}
