using Push.Shopify.Credentials;
using Push.Shopify.HttpClient;
using Push.Shopify.HttpClient.Credentials;

namespace Push.Shopify.Config
{
    public class ShopifySecuritySettings
    {
        public string ApiKey { get; set; }
        public string ApiPassword { get; set; }
        public string ApiSecret { get; set; }
        public string PrivateAppDomain { get; set; }
        public string EncryptKey { get; set; }
        public string EncryptIv { get; set; }


        // Need this for serialization functions
        public ShopifySecuritySettings()
        {     
        }

        public static ShopifySecuritySettings FromConfiguration()
        {
            var config = ShopifySecurityConfig.Settings;
            var output = new ShopifySecuritySettings()
            {
                ApiKey = config.ApiKey,
                ApiPassword = config.ApiPassword,
                ApiSecret = config.ApiSecret,
                EncryptKey = config.EncryptKey,
                EncryptIv = config.EncryptIv,
                PrivateAppDomain = config.PrivateAppDomain,
            };
            return output;
        }
        
        public ApiKeyAndSecret MakeApiKeyAndSecret(ShopDomain domain)
        {
            return new ApiKeyAndSecret(ApiKey, ApiSecret, domain);
        }

        public PrivateAppCredentials MakePrivateAppCredentials()
        {
            var domain = new ShopDomain(PrivateAppDomain);
            return new PrivateAppCredentials(ApiKey, ApiPassword, domain);
        }
    }
}
