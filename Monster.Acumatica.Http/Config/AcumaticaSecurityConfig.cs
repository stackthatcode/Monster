using System.Collections;
using System.Configuration;
using Push.Foundation.Utilities.Security;

namespace Monster.Acumatica.Config
{
    public class AcumaticaSecurityConfig
    {
        private static readonly
                Hashtable _settings =
                    (Hashtable)ConfigurationManager.GetSection("acumaticaSecurityConfiguration");

        public static AcumaticaSecurityConfig Settings { get; } = new AcumaticaSecurityConfig();
        
        
        [ConfigurationProperty("Branch", IsRequired = false)]
        public string Branch
        {
            get { return (string)_settings["Branch"]; }
            set { _settings["Branch"] = value; }
        }
        
        [ConfigurationProperty("CompanyName", IsRequired = false)]
        public string CompanyName
        {
            get { return (string)_settings["CompanyName"]; }
            set { _settings["CompanyName"] = value; }
        }

        [ConfigurationProperty("Username", IsRequired = false)]
        public string Username
        {
            get { return ((string)_settings["Username"])
                                    .DpApiDecryptString()
                                    .ToInsecureString(); }
            set { _settings["Username"] = value; }
        }

        [ConfigurationProperty("Password", IsRequired = false)]
        public string Password
        {
            get
            {
                return ((string)_settings["Password"])
                                    .DpApiDecryptString()
                                    .ToInsecureString();
            }

            set { _settings["Password"] = value; }
        }


        [ConfigurationProperty("InstanceUrl", IsRequired = false)]
        public string InstanceUrl
        {
            get { return ((string) _settings["InstanceUrl"]); }
            set { _settings["InstanceUrl"] = value; }
        }
    }
}

