using System.Collections;
using System.Configuration;
using Push.Foundation.Utilities.Helpers;

namespace Push.Shopify.Config
{
    public class ShopifyHttpConfig
    {
        private static readonly
                Hashtable _settings =
                    (Hashtable)ConfigurationManager
                        .GetSection("shopifyHttp");

        public static ShopifyHttpConfig Settings { get; } = new ShopifyHttpConfig();
        

        [ConfigurationProperty("MaxAttempts", IsRequired = true)]
        public int MaxAttempts
        {
            get { return ((string) _settings["MaxAttempts"]).ToIntegerAlt(3); }
            set { _settings["MaxAttempts"] = value; }
        }

        [ConfigurationProperty("Timeout", IsRequired = true)]
        public int Timeout
        {
            get { return ((string)_settings["Timeout"]).ToIntegerAlt(60000); }
            set { _settings["Timeout"] = value; }
        }

        [ConfigurationProperty("ThrottlingDelay", IsRequired = false)]
        public int ThrottlingDelay
        {
            get { return ((string)_settings["ThrottlingDelay"]).ToIntegerAlt(500); }
            set { _settings["ThrottlingDelay"] = value; }
        }
    }
}

