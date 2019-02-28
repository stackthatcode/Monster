using System.Collections;
using System.Configuration;
using Push.Foundation.Utilities.Helpers;

namespace Monster.Acumatica.Config
{
    public class AcumaticaHttpConfig
    {
        private static readonly
                Hashtable _settings =
                    (Hashtable)ConfigurationManager
                        .GetSection("acumaticaHttp");

        public static AcumaticaHttpConfig
                Settings { get; } = new AcumaticaHttpConfig();


        private const int DefaultMaxAttempts = 3;
        private const int DefaultTimeout = 180000;
        private const int DefaultThrottlingDelay = 250;
        private const string DefaultVersionSegment = "/entity/Monster/17.200.001/";


        [ConfigurationProperty("MaxAttempts", IsRequired = true)]
        public int MaxAttempts
        {
            get { return ((string)_settings["MaxAttempts"])
                        .ToIntegerAlt(DefaultMaxAttempts); }

            set { _settings["MaxAttempts"] = value; }
        }

        [ConfigurationProperty("Timeout", IsRequired = true)]
        public int Timeout
        {
            get { return ((string)_settings["Timeout"])
                        .ToIntegerAlt(DefaultTimeout); }

            set { _settings["Timeout"] = value; }
        }

        [ConfigurationProperty("ThrottlingDelay", IsRequired = false)]
        public int ThrottlingDelay
        {
            get { return ((string)_settings["ThrottlingDelay"])
                        .ToIntegerAlt(DefaultThrottlingDelay); }
            set { _settings["ThrottlingDelay"] = value; }
        }

        [ConfigurationProperty("VersionSegment", IsRequired = false)]
        public string VersionSegment
        {
            get { return ((string) _settings["VersionSegment"])
                            .IsNullOrEmptyAlt(DefaultVersionSegment); }

            set { _settings["VersionSegment"] = value; }
        }
    }
}

