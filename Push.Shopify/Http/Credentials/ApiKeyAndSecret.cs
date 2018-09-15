namespace Push.Shopify.Http.Credentials
{
    //
    // Used by Shopify for OAuth authentication
    //
    public class ApiKeyAndSecret : IShopifyCredentials
    {
        public string ApiKey { get; set; }
        public string ApiSecret { get; set; }
        public ShopDomain Domain { get; set; }


        public ApiKeyAndSecret(string apiKey, string apiSecret, ShopDomain domain)
        {
            ApiKey = apiKey;
            ApiSecret = apiSecret;
            Domain = domain;
        }
    }
}
