using Push.Foundation.Web.Http;

namespace Push.Shopify.Config
{
    public class ShopifyHttpSettings : HttpSettings
    {
        public ShopifyHttpSettings()
        {
            RetryLimit = 3;
            Timeout = 60000;
            ThrottlingDelay = 500;
        }

        public static ShopifyHttpSettings FromConfig()
        {
            var settings = ShopifyHttpConfig.Settings;
            var output = new ShopifyHttpSettings();
            output.RetryLimit = settings.RetryLimit;
            output.Timeout = settings.Timeout;
            output.ThrottlingDelay = settings.ThrottlingDelay;
            return output;
        }
    }
}

