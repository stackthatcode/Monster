using Monster.Acumatica.Http;

namespace Monster.Middle.Processes.Payouts
{
    public class PayoutConfig
    {
        public string ScreenApiUrl { get; set; }
        public AcumaticaCredentials Credentials { get; set; }
        public int ShopifyRecordsPerPage { get; set; }
        
        public PayoutConfig()
        {
            ShopifyRecordsPerPage = 250;
        }
    }
}
