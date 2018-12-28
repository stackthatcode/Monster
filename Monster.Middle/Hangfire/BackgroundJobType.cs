namespace Monster.Middle.Hangfire
{
    public class BackgroundJobType
    {
        public const int ConnectToAcumatica = 1;
        public const int PullAcumaticaReferenceData = 2;
        public const int SyncWarehouseAndLocation = 3;
        public const int PushInventoryToAcumatica = 4;
        public const int PushInventoryToShopify = 5;
        public const int RealTimeSync = 6;
    }
}
