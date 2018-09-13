using Push.Foundation.Web.HttpClient;

namespace Monster.Acumatica.Config
{
    public class AcumaticaHttpSettings : HttpSettings
    {
        public string VersionSegment = "/entity/Monster/17.200.001/";

        public AcumaticaHttpSettings()
        {
            RetryLimit = 3;
            Timeout = 180000;
            ThrottlingDelay = 250;
        }
    }
}
