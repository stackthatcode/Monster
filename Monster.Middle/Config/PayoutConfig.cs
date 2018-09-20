namespace Monster.Middle.Config
{
    public class PayoutConfig
    {
        public string ConnectionString { get; set; }
        public string ScreenApiUrl { get; set; }
        public int ShopifyRecordsPerPage { get; set; }
        public int ShopifyMaxPages { get; set; }

        public PayoutConfig()
        {
            ShopifyRecordsPerPage = 10;
            ShopifyMaxPages = 1;
        }
    }
}
