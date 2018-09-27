using System.Collections;
using System.Configuration;
using Push.Foundation.Utilities.Helpers;
using Push.Foundation.Utilities.Security;

namespace Monster.Acumatica.Config
{
    public class AcumaticaHttpConfig
    {
        private static readonly
                Hashtable _settings =
                    (Hashtable)ConfigurationManager
                        .GetSection("acumaticaHttpConfig");

        public static AcumaticaHttpConfig
                Settings { get; } = new AcumaticaHttpConfig();
        

        [ConfigurationProperty("RetryLimit", IsRequired = true)]
        public int RetryLimit
        {
            get { return ((string)_settings["RetryLimit"]).ToIntegerAlt(3); }
            set { _settings["RetryLimit"] = value; }
        }

        [ConfigurationProperty("Timeout", IsRequired = true)]
        public int Timeout
        {
            get { return ((string)_settings["RetryLimit"]).ToIntegerAlt(180000); }
            set { _settings["TimeoutTimeout"] = value; }
        }

        [ConfigurationProperty("ThrottlingDelay", IsRequired = false)]
        public int ThrottlingDelay
        {
            get { return ((string)_settings["ThrottlingDelay"]).ToIntegerAlt(0); }
            set { _settings["ThrottlingDelay"] = value; }
        }

        [ConfigurationProperty("VersionSegment", IsRequired = false)]
        public string VersionSegment
        {
            get { return ((string) _settings["VersionSegment"]); }
            set { _settings["VersionSegment"] = value; }
        }
    }
}

