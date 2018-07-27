using Push.Shopify.Config;
using Push.Shopify.HttpClient;
using Push.Shopify.HttpClient.Credentials;

namespace Push.Shopify.Credentials
{
    public static class ExtensionMethods
    {
        public static IShopifyCredentials ToPrivateAppCredentials(this ShopifySecurityConfig config)
        {
            var domain = new ShopDomain(config.PrivateAppDomain);
            return new ApiKeyAndSecret(config.ApiKey, config.ApiSecret, domain);
        }
    }
}
