using System.Collections;
using System.Configuration;
using Push.Foundation.Utilities.Security;

namespace Push.Shopify.Config
{
    public class ShopifySecurityConfiguration : ConfigurationSection
    {
        private static readonly
            Hashtable _settings =
                (Hashtable) ConfigurationManager.GetSection("shopifySecurityConfiguration") ?? new Hashtable();

        public static 
            ShopifySecurityConfiguration Settings
                { get; } = new ShopifySecurityConfiguration();


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
        
        // 
        // Convenience properties for storing encryption key and IV, 
        // ... ostensibly for safely storing OAuth Access Tokens
        //
        [ConfigurationProperty("EncryptKey", IsRequired = false)]
        public string EncryptKey
        {
            get { return ((string)_settings["EncryptKey"]).DpApiDecryptString().ToInsecureString(); }
            set { this["EncryptKey"] = value; }
        }

        [ConfigurationProperty("EncryptIv", IsRequired = false)]
        public string EncryptIv
        {
            get { return ((string)_settings["EncryptIv"]).DpApiDecryptString().ToInsecureString(); }
            set { this["EncryptIv"] = value; }
        }
    }
}
