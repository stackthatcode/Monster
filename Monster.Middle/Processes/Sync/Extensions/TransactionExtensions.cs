using Monster.Middle.Persist.Multitenant;
using Push.Shopify.Api.Transactions;

namespace Monster.Middle.Processes.Sync.Extensions
{
    public static class TransactionExtensions
    {

        // Only create Payments for Capture/Sale or Refund
        public static bool ShouldCreatePayment(this UsrShopifyTransaction input)
        {
            return input.ShopifyGateway != Gateway.Manual
                   && input.UsrShopifyAcuPayment == null
                   && input.ShopifyStatus == TransactionStatus.Success
                   && (input.ShopifyKind == TransactionKind.Capture
                       || input.ShopifyKind == TransactionKind.Sale);
        }


        // 
        public static bool ShouldCreateRefundPayment(this UsrShopifyTransaction input)
        {
            return input.ShopifyGateway != Gateway.Manual
                   && input.UsrShopifyAcuPayment == null
                   && input.ShopifyStatus == TransactionStatus.Success
                   && (input.ShopifyKind == TransactionKind.Refund);
        }
    }
}
