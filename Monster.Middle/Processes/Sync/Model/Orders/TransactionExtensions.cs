using Monster.Middle.Persist.Instance;

namespace Monster.Middle.Processes.Sync.Model.Orders
{
    public static class TransactionExtensions
    {
        public static bool HasBeenSynced(this ShopifyTransaction transaction)
        {
            return transaction.ShopifyAcuPayment != null;
        }
    }
}
