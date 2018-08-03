using Push.Shopify.Config;
using Push.Shopify.HttpClient;
using Push.Shopify.HttpClient.Credentials;

namespace Push.Shopify.Credentials
{
    public static class ExtensionMethods
    {
        public static IShopifyCredentials ToPrivateAppCredentials(this ShopifySecurityConfiguration configuration)
        {
            var domain = new ShopDomain(configuration.PrivateAppDomain);
            return new ApiKeyAndSecret(configuration.ApiKey, configuration.ApiSecret, domain);
        }
    }
}
