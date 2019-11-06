using Monster.Middle.Persist.Instance;
using Monster.Middle.Processes.Shopify.Persist;
using Monster.Middle.Processes.Sync.Model.Orders;
using Push.Foundation.Utilities.Validation;
using Push.Shopify.Api.Shop;
using Push.Shopify.Api.Transactions;

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
                .Add(x => ThisTransaction.ShopifyGateway != Gateway.Manual, $"Payment is manual")
                .Add(x => ThisTransaction.ShopifyStatus == TransactionStatus.Success, 
                            $"Transaction -> Status is {ThisTransaction.ShopifyStatus}");
        }

        public ValidationResult ShouldCreatePayment()
        {
            var validation
                = BuildBaseValidation()
                    .Add(x => !ThisTransaction.IsSynced(), "Payment has been synced already")
                    .Add(x => ThisTransaction.ShopifyKind == TransactionKind.Capture
                              || ThisTransaction.ShopifyKind == TransactionKind.Sale,
                            $"Transaction -> Kind is not a Capture or Sale");

            return validation.Run(this);
        }
        
        public ValidationResult ShouldCreateRefundPayment()
        {
            var validation
                = BuildBaseValidation()
                    .Add(x => !ThisTransaction.IsSynced(), "Refund has been synced already")
                    .Add(x => PaymentTransaction.IsSynced(), $"Original Payment has not been synced yet")
                    .Add(x => ThisTransaction.ShopifyKind == TransactionKind.Refund,
                            $"Transaction Kind is not a Capture or Sale");

            return validation.Run(this);
        }

        public ValidationResult ShouldRelease()
        {
            var validation = BuildBaseValidation()
                .Add(x => x.ThisTransaction.IsSynced(), "Transaction has not been synced to Acumatica yet")
                .Add(x => x.ThisTransaction.AcumaticaPayment != null &&
                          x.ThisTransaction.AcumaticaPayment.IsReleased == false,
                    "This Transaction has been release already");

            return validation.Run(this);
        }
    }
}
