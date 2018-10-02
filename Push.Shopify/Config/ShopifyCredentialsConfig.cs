using System.Collections;
using System.Configuration;
using Push.Foundation.Utilities.Security;
using Push.Shopify.Http;
using Push.Shopify.Http.Credentials;

namespace Push.Shopify.Config
{
    public class ShopifyCredentialsConfig : ConfigurationSection
    {
        private static readonly
            Hashtable _settings =
                (Hashtable) ConfigurationManager
                    .GetSection("shopifyCredentials") ?? new Hashtable();

        public static 
            ShopifyCredentialsConfig Settings
                { get; } = new ShopifyCredentialsConfig();


        [ConfigurationProperty("ApiKey", IsRequired = true)]
        public string ApiKey
        {
            get { return ((string)_settings["ApiKey"]).DpApiDecryptString().ToInsecureString(); }
            set { this["ApiKey"] = value; }
        }
        
        [ConfigurationProperty("ApiPassword", IsRequired = true)]
        public string ApiPassword
        {
            get { return ((string)_settings["ApiPassword"]).DpApiDecryptString().ToInsecureString(); }
            set { this["ApiPassword"] = value; }
        }

        [ConfigurationProperty("ApiSecret", IsRequired = true)]
        public string ApiSecret
        {
            get { return ((string)_settings["ApiSecret"]).DpApiDecryptString().ToInsecureString(); }
            set { this["ApiSecret"] = value; }
        }
        
        // 
        // Only needs to be set for Private Apps
        //  
        [ConfigurationProperty("PrivateAppDomain", IsRequired = false)]
        public string PrivateAppDomain
        {
            get { return (string)_settings["PrivateAppDomain"]; }
            set { this["PrivateAppDomain"] = value; }
        }
        


        // Functions for translating...
        public PrivateAppCredentials ToPrivateAppCredentials()
        {
            var domain = new ShopDomain(PrivateAppDomain);
            return new PrivateAppCredentials(ApiKey, ApiPassword, domain);
        }
    }
}
