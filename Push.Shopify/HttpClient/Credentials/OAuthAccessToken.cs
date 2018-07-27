namespace Push.Shopify.HttpClient.Credentials
{
    // 
    // NOTE - relies on a service that provides secure, external storage of 
    // ... AccessToken and ShopDomain. The Encryption Key and IV are suggested
    // ... for use with AesEncryptionService for securing the values
    // 
    public class OAuthAccessToken : IShopifyCredentials
    {
        public ShopDomain Domain { get; set; }
        public string AccessToken { get; set; }

        public OAuthAccessToken(string domain, string accessToken)
        {
            Domain = new ShopDomain(domain);
            AccessToken = accessToken;
        }
    }
}

