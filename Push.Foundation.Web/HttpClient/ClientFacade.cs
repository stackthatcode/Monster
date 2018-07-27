using System;
using System.Net;
using Push.Foundation.Utilities.Logging;
using Push.Foundation.Web.Misc;

namespace Push.Foundation.Web.HttpClient
{
    public class ClientFacade
    {
        private readonly HttpWebRequestProcessor _requestProcessor;
        private ClientSettings _settings;
        private readonly Throttler _throttler;
        private readonly InsistentExecutor _insistentExecutor;
        private readonly IPushLogger _pushLogger;
        

        public ClientFacade(
                HttpWebRequestProcessor requestProcessor, 
                ClientSettings settings,
                Throttler throttler,
                InsistentExecutor insistentExecutor,
                IPushLogger logger)
        {
            _requestProcessor = requestProcessor;
            _settings = settings;

            _throttler = throttler;
            throttler.TimeBetweenCallsMs = _settings.ThrottlingDelay;

            _insistentExecutor = insistentExecutor;
            _insistentExecutor.MaxNumberOfAttempts = _settings.RetryLimit;

            _pushLogger = logger;
        }

        public ClientSettings Settings
        {
            get { return _settings; }
            set { _settings = value; }
        }

        public virtual ResponseEnvelope ExecuteRequest(HttpWebRequest request)
        {
            if (_settings.RetriesEnabled)
            {
                // Only invoke via lambda if retries are enabled
                return _insistentExecutor.Execute(() => HttpInvocation(request));
            }
            else
            {
                return HttpInvocation(request);
            }
        }

        // NOTE: all HTTP calls must be routed through this method
        private ResponseEnvelope HttpInvocation(HttpWebRequest request)
        {
            _pushLogger.Debug(
                $"Invoking HTTP {request.Method} on {request.RequestUri.AbsoluteUri}");

            var hostname = request.RequestUri.Host;

            // Invoke the Throttler
            _throttler.Process(hostname);
            
            // Execute Request and process the HTTP Status Codes
            var startTime = DateTime.UtcNow;
            var response = _requestProcessor.Execute(request);
            
            // Log execution time            
            var executionTime = DateTime.UtcNow - startTime;
            _pushLogger.Debug($"Call performance - {executionTime} ms");
            
            return response.ProcessStatusCodes();
        }
    }
}


