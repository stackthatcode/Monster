using Monster.Middle.Persist.Instance;
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


        public static PaymentSyncStatus Make(ShopifyTransaction thisTransaction, ShopifyTransaction paymentTransaction)
        {
            return new PaymentSyncStatus
            {
                ThisTransaction = thisTransaction,
                PaymentTransaction = paymentTransaction,
            };
        }


        private Validation<PaymentSyncStatus> MakeBaseValidation()
        {
            return new Validation<PaymentSyncStatus>()
                .Add(x => !ThisTransaction.HasBeenSynced(), "Payment has been synced already")
                .Add(x => ThisTransaction.ShopifyGateway != Gateway.Manual, $"Payment is manual")
                .Add(x => ThisTransaction.ShopifyStatus == TransactionStatus.Success, 
                            $"Transaction Status is {ThisTransaction.ShopifyStatus}");
        }

        public ValidationResult ShouldCreatePayment()
        {
            var validation
                = MakeBaseValidation()
                    .Add(x => ThisTransaction.ShopifyKind == TransactionKind.Capture
                              || ThisTransaction.ShopifyKind == TransactionKind.Sale,
                        $"Transaction Kind is not a Capture or Sale");

            return validation.Run(this);
        }
        
        public ValidationResult ShouldCreateRefundPayment()
        {
            var validation
                = MakeBaseValidation()
                    .Add(x => PaymentTransaction.HasBeenSynced(), 
                        $"Original Payment has not been synced yet")
                    .Add(x => ThisTransaction.ShopifyKind == TransactionKind.Refund,
                        $"Transaction Kind is not a Capture or Sale");

            return validation.Run(this);
        }
    }
}
