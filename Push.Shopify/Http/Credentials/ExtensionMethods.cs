using Push.Shopify.Config;

namespace Push.Shopify.Http.Credentials
{
    public static class ExtensionMethods
    {
        public static IShopifyCredentials 
                ToKeyAndSecretCredentials(this ShopifySecurityConfiguration configuration)
        {
            var domain = new ShopDomain(configuration.PrivateAppDomain);
            return new PrivateAppCredentials(
                configuration.ApiKey, configuration.ApiSecret, domain);
        }
    }
}
