namespace Monster.Middle.Hangfire
{
    public class JobType
    {
        public const int ConnectToAcumatica = 1;
        public const int PullAcumaticaRefData = 2;
        public const int SyncWarehouseAndLocation = 3;
        public const int Diagnostics = 4;
        public const int PullInventoryFromShopifyAcumatica = 5;
    }
}
