using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using Push.Foundation.Utilities.Execution;
using Push.Foundation.Utilities.Logging;

namespace Push.Foundation.Utilities.Http
{
    public class HttpFacade : IDisposable
    {
        private readonly IPushLogger _logger;
        private readonly HttpClient _client;
        private readonly FaultTolerantExecutor _executor;

        public HttpFacade(
                IPushLogger logger, HttpClient client, FaultTolerantExecutor executor)
        {
            _logger = logger;
            _client = client;
            _executor = executor;
        }

        public virtual ResponseEnvelope Get(
                string url,
                Dictionary<string, string> headers = null,
                string contentType = "application/json; charset=utf-8")
        {
            _logger.Debug($"HTTP GET on {url}");

            var response = _executor.Do(() => _client.GetAsync(url).Result);
            
            return response.ToEnvelope();
        }

        public virtual ResponseEnvelope Post(string url, string content)
        {
            _logger.Debug($"HTTP POST on {url}");

            var httpContent = new StringContent(content, Encoding.UTF8, "application/json");

            var response = _executor.Do(() => _client.PostAsync(url, httpContent).Result);

            return response.ToEnvelope();
        }

        public virtual ResponseEnvelope Put(string url, string content)
        {
            _logger.Debug($"HTTP PUT on {url}");

            var httpContent = new StringContent(content, Encoding.UTF8, "application/json");

            var response = _executor.Do(() => _client.PutAsync(url, httpContent).Result);
            
            return response.ToEnvelope();
        }

        public void Dispose()
        {
            _client?.Dispose();
        }
    }
}
