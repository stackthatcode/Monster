using Push.Shopify.Http;
using Push.Shopify.Http.Credentials;

namespace Push.Shopify.Config
{
    public class ShopifyCredentials
    {
        public string ApiKey { get; set; }
        public string ApiPassword { get; set; }
        public string ApiSecret { get; set; }
        public string Domain { get; set; }

        // DO NOT REMOVE - need this for serialization functions
        public ShopifyCredentials()
        {     
        }

        public static ShopifyCredentials FromConfiguration()
        {
            var config = ShopifyCredentialsConfig.Settings;
            var output = new ShopifyCredentials()
            {
                ApiKey = config.ApiKey,
                ApiPassword = config.ApiPassword,
                ApiSecret = config.ApiSecret,
                Domain = config.PrivateAppDomain,
            };
            return output;
        }
        
        public ApiKeyAndSecret MakeApiKeyAndSecret(ShopDomain domain)
        {
            return new ApiKeyAndSecret(ApiKey, ApiSecret, domain);
        }

        public PrivateAppCredentials MakePrivateAppCredentials()
        {
            var domain = new ShopDomain(Domain);
            return new PrivateAppCredentials(ApiKey, ApiPassword, domain);
        }
    }
}
