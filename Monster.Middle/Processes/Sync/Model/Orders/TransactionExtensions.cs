using Monster.Middle.Persist.Instance;

namespace Monster.Middle.Processes.Sync.Model.Orders
{
    public static class TransactionExtensions
    {
        public static bool IsSynced(this ShopifyTransaction transaction)
        {
            return transaction.AcumaticaPayment != null;
        }

        public static bool IsReleased(this ShopifyTransaction transaction)
        {
            return transaction.AcumaticaPayment != null && transaction.AcumaticaPayment.IsReleased;
        }
    }
}
