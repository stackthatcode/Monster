using Push.Foundation.Web.Http;

namespace Monster.Acumatica.Config
{
    public class AcumaticaHttpSettings : HttpSettings
    {
        public string VersionSegment { get; set; }
        
        public AcumaticaHttpSettings()
        {
            RetryLimit = 3;
            Timeout = 180000;
            ThrottlingDelay = 250;
            VersionSegment = "/entity/Monster/17.200.001/";
        }

        public static AcumaticaHttpSettings FromConfig()
        {
            var settings 
                = AcumaticaHttpConfig.Settings;
                    return new AcumaticaHttpSettings
                    {
                        RetryLimit = settings.RetryLimit,
                        ThrottlingDelay = settings.ThrottlingDelay,
                        Timeout = settings.Timeout,
                        VersionSegment = settings.VersionSegment,
                    };
        }
    }
}
