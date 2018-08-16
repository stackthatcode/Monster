namespace Push.Shopify.HttpClient.Credentials
{
    // Used exclusively by private apps
    public class PrivateAppCredentials : IShopifyCredentials
    {
        public string ApiKey { get; set; }
        public string ApiPassword { get; set; }
        public ShopDomain Domain { get; set; }


        public PrivateAppCredentials(string apiKey, string apiPassword, ShopDomain domain)
        {
            ApiKey = apiKey;
            ApiPassword = apiPassword;
            Domain = domain;
        }
    }
}
