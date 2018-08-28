using Push.Foundation.Web.HttpClient;

namespace Monster.Acumatica.Http
{
    public class AcumaticaHttpSettings : HttpSettings
    {
        public AcumaticaHttpSettings()
        {
            RetryLimit = 3;
            Timeout = 180000;
            ThrottlingDelay = 250;
        }
    }
}
