using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Monster.Acumatica.Config;
using Push.Foundation.Utilities.Execution;
using Push.Foundation.Utilities.Http;
using Push.Foundation.Utilities.Logging;

namespace Monster.Acumatica.Http
{
    public class AcumaticaHttpContext : IDisposable
    {
        public readonly Guid ObjectIdentifier = Guid.NewGuid();

        private readonly AcumaticaHttpConfig _settings;
        private readonly IPushLogger _logger;

        // These are set by the Initialize method
        private string _instanceUrl;
        private HttpClient _httpClient;
        private FaultTolerantExecutor _executor;
        private AcumaticaCredentials _credentials;
        public Uri BaseAddress { private set; get; }

        public bool IsLoggedIn { get; private set; } = false;

        public AcumaticaHttpContext(AcumaticaHttpConfig settings, IPushLogger logger)
        {
            _settings = settings;
            _logger = logger;
        }

        public void Initialize(AcumaticaCredentials credentials)
        {
            _credentials = credentials;

            BaseAddress = new Uri(credentials.InstanceUrl);
            _instanceUrl = credentials.InstanceUrl;

            _executor = new FaultTolerantExecutor()
            {
                MaxNumberOfAttempts = _settings.MaxAttempts,
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
            var response = Post(path, content, excludeVersion: true);
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
            // Arrange
            var address = MakePath(path, excludeVersion);
            var urlDebug = $"HTTP GET on {address} (ContextId: {this.ObjectIdentifier})";
            var errorContext = BuildErrorContext(urlDebug);
            _logger.Debug(urlDebug);

            // Act
            var response = _executor.Do(() => _httpClient.GetAsync(address).Result, errorContext);
            
            // Response
            var output = response.ToEnvelope();
            _logger.Trace(output.Body);

            // Assert
            return ProcessStatusCodes(output, urlDebug);
        }

        public ResponseEnvelope Post(
                string path, 
                string content, 
                Dictionary<string, string> headers = null,
                bool excludeVersion = false)
        {
            // Arrange
            var address = MakePath(path, excludeVersion);
            var urlDebug = $"HTTP POST on {address} (ContextId: {this.ObjectIdentifier})";
            var errorContext = BuildErrorContext(urlDebug, content);
            _logger.Debug(urlDebug);
            
            _logger.Trace(content);
            var httpContent = new StringContent(content, Encoding.UTF8, "application/json");
            
            // Act
            var response = 
                _executor.Do(
                    () => _httpClient.PostAsync(address, httpContent).Result, errorContext);

            // Response
            var output = response.ToEnvelope();
            _logger.Trace(output.Body);

            // Assert
            return ProcessStatusCodes(output, errorContext);
        }

        public ResponseEnvelope Put(
                string path,
                string content,
                Dictionary<string, string> headers = null,
                bool excludeVersion = false)
        {
            var address = MakePath(path, excludeVersion);

            var urlDebug = $"HTTP PUT on {address} (ContextId: {this.ObjectIdentifier})";
            var errorContext = BuildErrorContext(urlDebug, content);

            _logger.Debug(urlDebug);
            _logger.Trace(content);

            var httpContent = new StringContent(content, Encoding.UTF8, "application/json");

            var response =
                _executor.Do(
                    () => _httpClient.PutAsync(address, httpContent).Result, errorContext);

            var output = response.ToEnvelope();

            _logger.Trace(output.Body);

            return ProcessStatusCodes(output, errorContext);
        }

        private string BuildErrorContext(string url, string requestBody = null)
        {
            var output = $"*** Failed HTTP Request: {url}";

            if (requestBody != null)
            {
                output += Environment.NewLine + requestBody;
            }

            return output;
        }

        public ResponseEnvelope ProcessStatusCodes(ResponseEnvelope response, string errorContext)
        {
            // All other non-200 calls throw an exception
            //
            if (response.HasBadStatusCode)
            {
                throw new Exception(
                    $"Bad Status Code - HTTP {(int)response.StatusCode} ({response.StatusCode})"
                    + Environment.NewLine + errorContext);
            }

            return response;
        }

        public void SessionRun(Action action)
        {
            try
            {
                if (!IsLoggedIn)
                {
                    Login();
                }

                action();
            }
            finally
            {
                if (IsLoggedIn)
                {
                    Logout();
                }
            }
        }


        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}
