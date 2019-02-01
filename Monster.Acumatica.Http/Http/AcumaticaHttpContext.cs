using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Monster.Acumatica.Config;
using Push.Foundation.Utilities.Logging;
using Push.Foundation.Web.Http;
using Push.Foundation.Web.Misc;

namespace Monster.Acumatica.Http
{
    public class AcumaticaHttpContext : IDisposable
    {
        public readonly Guid Identifier = Guid.NewGuid();

        private readonly AcumaticaHttpConfig _settings;
        private readonly IPushLogger _logger;

        // These are set by the Initialize method
        private string _instanceUrl;        
        private HttpClient _httpClient;
        private ExecutorContext _executorContext;
        private AcumaticaCredentials _credentials;
        public Uri BaseAddress { private set; get; }

        public bool IsLoggedIn { get; private set; } = false;

        public AcumaticaHttpContext(
                AcumaticaHttpConfig settings, 
                IPushLogger logger)
        {
            _settings = settings;
            _logger = logger;
        }
        
        public void Initialize(AcumaticaCredentials credentials)
        {
            _credentials = credentials;

            BaseAddress = new Uri(credentials.InstanceUrl);
            _instanceUrl = credentials.InstanceUrl;

            _executorContext = new ExecutorContext()
            {
                NumberOfAttempts = _settings.RetryLimit,
                ThrottlingKey = credentials.InstanceUrl,
                Logger = _logger,
            };

            _httpClient 
                = new HttpClient(
                    new HttpClientHandler
                    {
                        UseCookies = true,
                        CookieContainer = new CookieContainer(),
                    })
                    {
                        BaseAddress = BaseAddress,
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
            _httpClient.Timeout = new TimeSpan(0, 0, 0, _settings.Timeout);
        }


        public void Login()
        {
            var path = $"/entity/auth/login";
            var content = _credentials.AuthenticationJson;
            var response = Post(path, content, excludeVersion:true);
            IsLoggedIn = true;
        }

        public void Logout()
        {
            var path = $"/entity/auth/logout";
            var response = Post(path, "", excludeVersion: true);
            IsLoggedIn = false;
        }

        public string MakePath(string path, bool excludeVersion = false)
        {
            return !excludeVersion
                ? $"{_instanceUrl}{_settings.VersionSegment}{path}" 
                : $"{_instanceUrl}{path}";
        }

        public ResponseEnvelope Get(
                    string path,
                    Dictionary<string, string> headers = null,
                    bool excludeVersion = false)
        {
            var address = MakePath(path, excludeVersion);
            _logger.Debug($"HTTP GET on {address}");

            var response =
                DurableExecutor.Do(
                    () => _httpClient.GetAsync(address).Result, _executorContext);

            var output = response.ToEnvelope();
            _logger.Trace(output.Body);
            output.ProcessStatusCodes();
            return output;
        }

        public ResponseEnvelope Post(
                    string path, 
                    string content,
                    Dictionary<string, string> headers = null,
                    bool excludeVersion = false)
        {
            var httpContent
                = new StringContent(content, Encoding.UTF8, "application/json");

            var address = MakePath(path, excludeVersion);
            _logger.Debug($"HTTP POST on {address}");
            _logger.Trace(content);
            var response =
                DurableExecutor.Do(
                    () => _httpClient.PostAsync(address, httpContent).Result,
                    _executorContext);

            var output = response.ToEnvelope();
            _logger.Trace(output.Body);
            output.ProcessStatusCodes();
            return output;
        }

        public ResponseEnvelope Put(
                    string path, 
                    string content,
                    Dictionary<string, string> headers = null,
                    bool excludeVersion = false)
        {
            _logger.Debug($"HTTP PUT on {path}");
            _logger.Trace(content);

            var address = MakePath(path, excludeVersion);

            var httpContent
                = new StringContent(content, Encoding.UTF8, "application/json");

            var response =
                DurableExecutor.Do(
                    () => _httpClient.PutAsync(address, httpContent).Result,
                    _executorContext);

            var output = response.ToEnvelope();
            _logger.Trace(output.Body);
            output.ProcessStatusCodes();
            return output;
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}

