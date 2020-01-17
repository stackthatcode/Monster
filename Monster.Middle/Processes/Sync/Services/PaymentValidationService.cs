using System.Linq;
using Monster.Middle.Persist.Instance;
using Monster.Middle.Processes.Shopify.Persist;
using Monster.Middle.Processes.Sync.Misc;
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

        public void ValidatePayment(ShopifyOrder order, PaymentAction action)
        {
            action.Validation = new ValidationResult();

            var transaction = order
                .ShopifyTransactions
                .FirstOrDefault(x => x.ShopifyTransactionId == action.ShopifyTransactionId);

            if (transaction == null)
            {
                return;
            }
            
            if (action.ActionCode == ActionCode.CreateInAcumatica)
            {
                action.Validation = ReadyToCreatePayment(transaction);
                return;
            }

            if (action.ActionCode == ActionCode.UpdateInAcumatica)
            {
                action.Validation = ReadyToUpdatePayment(transaction);
                return;
            }

            if (action.ActionCode == ActionCode.ReleaseInAcumatica)
            {
                action.Validation = ReadyToRelease(transaction);
                return;
            }
        }

        public void ValidateRefundPayment(ShopifyOrder order, PaymentAction action)
        {
            var transaction = order
                .ShopifyTransactions
                .First(x => x.ShopifyTransactionId == action.ShopifyTransactionId);

            action.Validation = new ValidationResult();

            if (action.ActionCode == ActionCode.CreateInAcumatica)
            {
                action.Validation = ReadyToCreateRefundPayment(transaction);
            }

            if (action.ActionCode == ActionCode.ReleaseInAcumatica)
            {
                action.Validation = ReadyToReleaseRefundPayment(transaction);
            }
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
                .Add(x => x.CurrentTransaction.ShopifyOrderId < SystemConsts.ErrorThreshold,
                    "Encountered too many errors attempting to synchronize");
        }

        private ValidationResult ReadyToCreatePayment(ShopifyTransaction currentTransaction)
        {
            var context = BuildContext(currentTransaction);

            var validation
                = BuildBaseValidation()
                    .Add(x => x.ValidPaymentGateway, $"Does not have a valid payment gateway; please check configuration")
                    .Add(x => !x.CurrentTransaction.ExistsInAcumatica(), "Payment has already been created in Acumatica")
                    .Add(x => x.CurrentTransaction.IsPayment(), $"Transaction is not a capture or sale")
                    .Add(x => x.AcumaticaSalesOrder == null
                              || x.AcumaticaSalesOrder.AcumaticaIsTaxValid,
                            "Acumatica Sales Order Taxes are invalid");

            return validation.Run(context);
        }

        private ValidationResult ReadyToUpdatePayment(ShopifyTransaction currentTransaction)
        {
            var context = BuildContext(currentTransaction);

            var validation
                = BuildBaseValidation()
                    .Add(x => x.CurrentTransaction.NeedsPaymentPut, "Payment has already been updated in Acumatica")
                    .Add(x => x.CurrentTransaction.ExistsInAcumatica(), "Payment has not been synced")
                    .Add(x => x.CurrentTransaction.IsPayment(), $"Transaction is not a capture or sale");

            return validation.Run(context);
        }

        private ValidationResult ReadyToRelease(ShopifyTransaction currentTransaction)
        {
            var context = BuildContext(currentTransaction);

            var validation
                = BuildBaseValidation()
                    .Add(x => x.CurrentTransaction.ExistsInAcumatica(), "Transaction has not been synced to Acumatica yet")
                    .Add(x => !x.CurrentTransaction.IsReleased(), "This Transaction has been released already");

            return validation.Run(context);
        }

        private ValidationResult ReadyToCreateRefundPayment(ShopifyTransaction currentTransaction)
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

        private ValidationResult ReadyToReleaseRefundPayment(ShopifyTransaction currentTransaction)
        {
            var context = BuildContext(currentTransaction);

            var validation
                = BuildBaseValidation()
                    .Add(x => x.CurrentTransaction.ExistsInAcumatica(), "Refund has not been synced yet")
                    .Add(x => x.OriginalPaymentTransaction.ExistsInAcumatica(), $"Payment has not been synced yet")
                    .Add(x => x.CurrentTransaction.IsRefund(), $"Transaction Kind is not a refund");

            //.Add(x => x.CurrentTransaction.AcumaticaPayment.AcumaticaRefNbr != AcumaticaSyncConstants.UnknownRefNbr,
            //        $"Acumatica Payment/Customer Refund has unknown Reference Number");

            return validation.Run(context);
        }


        private ValidationResult ReadyToCreateMemo(RootAction rootActions)
        {
            var validation = new Validation<RootAction>()
                .Add(x => x.HasPendingOrderActions, "Cannot create Memos until Order is synced")
                .Add(x => x.HasPendingOrderActions, "Cannot create Memos until Payments/Refunds are synced");

            return validation.Run(rootActions);
        }
    }
}
