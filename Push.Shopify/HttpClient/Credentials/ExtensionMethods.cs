using Push.Shopify.Config;

namespace Push.Shopify.HttpClient.Credentials
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
