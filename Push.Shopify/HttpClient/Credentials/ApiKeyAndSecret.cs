namespace Push.Shopify.HttpClient.Credentials
{
    //
    // NOTE: use the ShopifySecuritySettings to spawn these
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
