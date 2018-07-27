using System.Collections;
using System.Configuration;
using Push.Foundation.Utilities.Helpers;

namespace Push.Shopify.Config
{
    public class ShopifyClientConfig
    {
        private static readonly
                Hashtable _settings =
                    (Hashtable)ConfigurationManager.GetSection("shopifyClientConfig");

        public static ShopifyClientConfig Settings { get; } = new ShopifyClientConfig();
        

        [ConfigurationProperty("RetryLimit", IsRequired = true)]
        public int RetryLimit
        {
            get { return ((string) _settings["RetryLimit"]).ToIntegerAlt(3); }
            set { _settings["RetryLimit"] = value; }
        }

        [ConfigurationProperty("Timeout", IsRequired = true)]
        public int Timeout
        {
            get { return ((string)_settings["RetryLimit"]).ToIntegerAlt(60000); }
            set { _settings["TimeoutTimeout"] = value; }
        }

        [ConfigurationProperty("ThrottlingDelay", IsRequired = false)]
        public int ThrottlingDelay
        {
            get { return ((string)_settings["ThrottlingDelay"]).ToIntegerAlt(500); }
            set { _settings["ThrottlingDelay"] = value; }
        }
    }
}

