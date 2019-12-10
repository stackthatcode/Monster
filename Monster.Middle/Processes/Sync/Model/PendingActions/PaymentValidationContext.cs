using Monster.Middle.Persist.Instance;
using Monster.Middle.Processes.Shopify.Persist;

namespace Monster.Middle.Processes.Sync.Model.PendingActions
{
    public class PaymentValidationContext
    {
        public ShopifyTransaction CurrentTransaction { get; set; }
        public ShopifyOrder ShopifyOrder => CurrentTransaction.ShopifyOrder;
        public ShopifyTransaction OriginalPaymentTransaction => ShopifyOrder.PaymentTransaction();
        public AcumaticaSalesOrder AcumaticaSalesOrder => ShopifyOrder.AcumaticaSalesOrder;
        public bool ValidPaymentGateway { get; set; }
    }
}
