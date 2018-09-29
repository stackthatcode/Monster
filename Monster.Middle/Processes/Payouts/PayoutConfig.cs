namespace Monster.Middle.Processes.Payouts
{
    public class PayoutConfig
    {
        public string ScreenApiUrl { get; set; }
        public int ShopifyRecordsPerPage { get; set; }
        public int ShopifyMaxPages { get; set; }

        public PayoutConfig()
        {
            ShopifyRecordsPerPage = 250;
            ShopifyMaxPages = 1;
        }
    }
}
