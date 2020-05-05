using System.Collections;
using System.Configuration;
using Push.Foundation.Utilities.Config;
using Push.Foundation.Utilities.General;

namespace Monster.Middle.Config
{    
    public class MonsterConfig : ConfigurationSection
    {
        private static readonly Hashtable _settings =
                (Hashtable)ConfigurationManager.GetSection("monsterConfig") ?? new Hashtable();

        public static MonsterConfig Settings { get; } = new MonsterConfig();


        [ConfigurationProperty("EncryptKey", IsRequired = false)]
        public string EncryptKey
        {
            get { return ((string)_settings["EncryptKey"]).DecryptConfig(); }
            set { this["EncryptKey"] = value; }
        }

        [ConfigurationProperty("EncryptIv", IsRequired = false)]
        public string EncryptIv
        {
            get { return ((string)_settings["EncryptIv"]).DecryptConfig(); }
            set { this["EncryptIv"] = value; }
        }
        
        [ConfigurationProperty("SystemDatabaseConnection", IsRequired = false)]
        public string SystemDatabaseConnection
        {
            get { return ((string) _settings["SystemDatabaseConnection"]); }
            set { this["SystemDatabaseConnection"] = value; }
        }

        [ConfigurationProperty("DisableShopifyPut", IsRequired = false)]
        public bool DisableShopifyPut
        {
            get => (_settings["DisableShopifyPut"].ToBoolTryParse());
            set => this["DisableShopifyPut"] = value;
        }
    }
}

