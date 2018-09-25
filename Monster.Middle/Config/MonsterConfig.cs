namespace Monster.Middle.Config
{
    public class MonsterConfig
    {
        public string ConnectionString { get; set; }
        public int ShopifyRecordsPerPage { get; set; }
        public int ShopifyMaxPages { get; set; }

        public MonsterConfig()
        {
            ShopifyRecordsPerPage = 10;
            ShopifyMaxPages = 1;
        }
    }
}
