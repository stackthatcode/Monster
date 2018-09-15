using System;
using System.Collections.Generic;
using Push.Foundation.Utilities.Logging;
using Push.Foundation.Web.Misc;


namespace Push.Foundation.Web.HttpClient
{
    public class HttpFacade
    {
        private HttpSettings _settings;
        private IRequestBuilder _requestBuilder;

        private readonly HttpWebRequestProcessor _requestProcessor;
        private readonly Throttler _throttler;
        private readonly InsistentExecutor _insistentExecutor;
        private readonly IPushLogger _pushLogger;

        // The settings and builder are what will be property injected
        // in the factory fo the Facade to dictate preferred behaviors
        public HttpFacade(
                HttpWebRequestProcessor requestProcessor, 
                Throttler throttler,
                InsistentExecutor insistentExecutor,
                IPushLogger logger)
        {
            
            _requestProcessor = requestProcessor;
            _throttler = throttler;
            _insistentExecutor = insistentExecutor;
            _pushLogger = logger;
        }
        

        public HttpFacade InjectRequestBuilder(IRequestBuilder builder)
        {
            _requestBuilder = builder;
            return this;
        }

        public HttpFacade InjectSettings(HttpSettings settings)
        {
            _settings = settings;
            return this;
        }


        public virtual ResponseEnvelope Get(
                string url,
                Dictionary<string, string> headers = null,
                string contentType = "application/json; charset=utf-8")
        {
            var request = 
                new RequestEnvelope(
                    "GET", url, contentType: contentType);

            return ExecuteRequestWithInsistence(request);
        }

        public virtual ResponseEnvelope Post(
                string url, 
                string content = null,
                Dictionary<string, string> headers = null,
                string contentType = "application/json; charset=utf-8")
        {
            var request = 
                new RequestEnvelope(
                    "POST", 
                    url, 
                    content: content, 
                    contentType: contentType, 
                    headers:headers);

            return ExecuteRequestWithInsistence(request);
        }

        public virtual ResponseEnvelope Put(
                string url, 
                string content = null, 
                Dictionary<string, string> headers = null,
                string contentType = "application/json; charset=utf-8")
        {
            var request =
                new RequestEnvelope(
                    "PUT",
                    url,
                    content: content,
                    contentType: contentType,
                    headers: headers);

            return ExecuteRequestWithInsistence(request);
        }

        public virtual ResponseEnvelope Delete(
                string url,
                Dictionary<string, string> headers = null)
        {
            var request =
                new RequestEnvelope("GET", url, headers: headers);
            return ExecuteRequestWithInsistence(request);
        }


        public virtual ResponseEnvelope ExecuteRequestWithInsistence(RequestEnvelope requestEnvelope)
        {
            AssertDependencyQuorum();

            _insistentExecutor.MaxNumberOfAttempts = _settings.RetryLimit;
            return _insistentExecutor
                .Execute(() => ExecuteRequest(requestEnvelope));
        }

        public virtual ResponseEnvelope 
                            ExecuteRequest(RequestEnvelope requestEnvelope)
        {
            AssertDependencyQuorum();

            _pushLogger.Debug(
                $"Invoking HTTP {requestEnvelope.Method} " +
                $"on {requestEnvelope.Url}");

            // Create HttpWebRequest
            var request = _requestBuilder.Make(requestEnvelope);
            var hostname = request.RequestUri.Host;

            // Invoke the Throttler
            _throttler.TimeBetweenCallsMs = _settings.ThrottlingDelay;
            _throttler.Process(hostname);
            
            // Execute Request and process the HTTP Status Codes
            var startTime = DateTime.UtcNow;
            var response = _requestProcessor.Execute(request);
            
            // Log execution time            
            var executionTime = DateTime.UtcNow - startTime;
            _pushLogger.Debug($"Call performance - {executionTime} ms");
            
            // Process status codes
            response.ProcessStatusCodes();

            return response;
        }

        private void AssertDependencyQuorum()
        {
            if (_requestBuilder == null)
            {
                throw new Exception("IRequestBuilder has not been populated via InjectRequestBuilder");
            }
            if (_settings == null)
            {
                throw new Exception("HttpSettings has not been populated via InjectSettings");
            }
        }
    }
}


