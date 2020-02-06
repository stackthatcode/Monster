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
            return transaction.AcumaticaPayment != null && !transaction.AcumaticaPayment.NeedRelease;
        }

        public static bool NeedsSync(this ShopifyTransaction transaction)
        {
            return transaction.NeedsSync() &&
                   (!transaction.ExistsInAcumatica() || !transaction.IsReleased());
        }

    }
}

