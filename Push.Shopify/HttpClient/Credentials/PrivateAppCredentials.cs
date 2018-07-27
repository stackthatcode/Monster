using Push.Shopify.HttpClient;
using Push.Shopify.HttpClient.Credentials;

namespace Push.Shopify.Credentials
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
