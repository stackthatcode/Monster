using Push.Foundation.Web.Http;

namespace Push.Shopify.Config
{
    public class ShopifyClientSettings : HttpSettings
    {
        public ShopifyClientSettings()
        {
            RetryLimit = 3;
            Timeout = 60000;
            ThrottlingDelay = 500;
        }

        public ShopifyClientSettings(ShopifyClientConfig configuration)
        {
            RetryLimit = configuration.RetryLimit;
            Timeout = configuration.Timeout;
            ThrottlingDelay = configuration.ThrottlingDelay;
        }
    }
}

