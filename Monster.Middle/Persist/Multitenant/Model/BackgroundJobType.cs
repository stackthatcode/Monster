namespace Monster.Middle.Persist.Multitenant.Model
{
    public class BackgroundJobType
    {
        public const int PullSettingsFromAcumatica = 1;
        public const int SyncWarehouseAndLocation = 2;
        public const int PushInventoryToAcumatica = 3;
        public const int PushInventoryToShopify = 4;
        public const int RealTimeSync = 5;
    }
}
