using Monster.Middle.Persist.Instance;
using Push.Foundation.Utilities.Validation;
using Push.Shopify.Api.Transactions;

namespace Monster.Middle.Processes.Sync.Model.Status
{
    public class PaymentSyncStatus
    {
        public long ShopifyTransactionId { get; set; }
        public long ShopifyOrderNumber { get; set; } 
        public string ShopifyKind { get; set; }
        public string ShopifyStatus { get; set; }
        public string ShopifyGateway { get; set; }
        public bool HasBeenSynced { get; set; }


        public static PaymentSyncStatus Make(ShopifyTransaction transaction)
        {
            var output = new PaymentSyncStatus();
            output.ShopifyTransactionId = transaction.ShopifyTransactionId;
            output.ShopifyOrderNumber = transaction.ShopifyOrder.ShopifyOrderNumber;
            output.ShopifyStatus = transaction.ShopifyStatus;
            output.ShopifyKind = transaction.ShopifyKind;
            output.ShopifyGateway = transaction.ShopifyGateway;
            output.HasBeenSynced = transaction.ShopifyAcuPayment != null;
            return output;
        }


        private Validation<PaymentSyncStatus> MakeBaseValidation()
        {
            return new Validation<PaymentSyncStatus>()
                .Add(x => !x.HasBeenSynced, "Payment has been synced already")
                .Add(x => x.ShopifyGateway != Gateway.Manual, $"Payment is manual")
                .Add(x => x.ShopifyStatus == TransactionStatus.Success, $"Transaction Status is {ShopifyStatus}");
        }

        public ValidationResult ShouldCreatePayment()
        {
            var validation
                = MakeBaseValidation()
                    .Add(x => x.ShopifyKind == TransactionKind.Capture
                              || x.ShopifyKind == TransactionKind.Sale,
                        $"Transaction Kind is not a Capture or Sale");

            return validation.Run(this);
        }
        
        public ValidationResult ShouldCreateRefundPayment()
        {
            var validation
                = MakeBaseValidation()
                    .Add(x => x.ShopifyKind == TransactionKind.Refund,
                        $"Transaction Kind is not a Capture or Sale");

            return validation.Run(this);
        }
    }
}
