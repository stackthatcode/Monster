namespace Monster.Middle.Persist.Multitenant.Model
{
    public class QueuedJobType
    {
        public const int SyncWarehouseAndLocation = 1;
        public const int LoadInventoryIntoAcumatica = 2;
        public const int LoadInventoryIntoShopify = 3;
    }
}
