using Monster.Middle.Persist.Instance;
using Monster.Middle.Processes.Shopify.Persist;

namespace Monster.Middle.Processes.Sync.Model.Orders
{
    public static class TransactionExtensions
    {
        public static bool ExistsInAcumatica(this ShopifyTransaction transaction)
        {
            return transaction.AcumaticaPayment != null;
        }

        public static bool IsReleased(this ShopifyTransaction transaction)
        {
            return transaction.AcumaticaPayment != null && transaction.AcumaticaPayment.IsReleased;
        }

        public static bool NeedToCreate(this ShopifyTransaction transaction)
        {
            return transaction.DoNotIgnore() && !transaction.ExistsInAcumatica();
        }

        public static bool NeedToUpdate(this ShopifyTransaction transaction)
        {
            return transaction.DoNotIgnore() &&
                   transaction.AcumaticaPayment != null
                   && transaction.NeedsPaymentPut;
        }

        public static bool NeedToRelease(this ShopifyTransaction transaction)
        {
            return transaction.DoNotIgnore() && !transaction.IsReleased();
        }
    }
}

