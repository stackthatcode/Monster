using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using Monster.Acumatica.Config;

namespace Monster.Acumatica.Http
{
    public class AcumaticaHttpClientFactory
    {
        private readonly SessionContainer _session;
        private readonly AcumaticaHttpSettings _settings;

        public AcumaticaHttpClientFactory(
                SessionContainer session,
                AcumaticaHttpSettings settings)
        {
            _session = session;
            _settings = settings;
        }
        
        public HttpClient Make(AcumaticaCredentials config)
        {
            var baseAddress = new Uri(config.InstanceUrl);
            var httpClient 
                = new HttpClient(
                    new HttpClientHandler
                    {
                        UseCookies = true,
                        CookieContainer = _session.CookieContainer,
                    })
                    {
                        BaseAddress = baseAddress,
                        DefaultRequestHeaders =
                        {
                            Accept =
                            {
                                MediaTypeWithQualityHeaderValue.Parse("text/json")
                            }
                        }
                    };

            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

            // Spawn the WebRequest
            httpClient.Timeout = new TimeSpan(0, 0, 0, _settings.Timeout);
            return httpClient;
        }
    }
}

