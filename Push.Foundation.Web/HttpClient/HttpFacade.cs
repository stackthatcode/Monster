using System;
using System.Collections.Generic;
using Push.Foundation.Utilities.Logging;
using Push.Foundation.Web.Misc;

namespace Push.Foundation.Web.HttpClient
{
    public class HttpFacade
    {
        private HttpSettings _settings;
        private readonly IRequestBuilder _requestFactory;

        private readonly HttpWebRequestProcessor _requestProcessor;
        private readonly Throttler _throttler;
        private readonly InsistentExecutor _insistentExecutor;
        private readonly IPushLogger _pushLogger;

        // The Request Factory and Settings are what will be injected
        // by consumers of the Facade to dictate preferred behaviors
        public HttpFacade(
                IRequestBuilder requestFactory,
                HttpSettings settings,
                
                HttpWebRequestProcessor requestProcessor, 
                Throttler throttler,
                InsistentExecutor insistentExecutor,
                IPushLogger logger)
        {
            _requestFactory = requestFactory;
            _settings = settings;


            _requestProcessor = requestProcessor;

            _throttler = throttler;
            throttler.TimeBetweenCallsMs = _settings.ThrottlingDelay;

            _insistentExecutor = insistentExecutor;
            _insistentExecutor.MaxNumberOfAttempts = _settings.RetryLimit;

            _pushLogger = logger;
        }

        public HttpSettings Settings
        {
            get { return _settings; }
            set { _settings = value; }
        }
        

        public virtual ResponseEnvelope Get(
                string url,
                Dictionary<string, string> headers = null,
                string contentType = "application/json")
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
                string contentType = "application/json")
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
                string contentType = "application/json")
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
            return _insistentExecutor
                .Execute(() => ExecuteRequest(requestEnvelope));
        }

        public virtual ResponseEnvelope ExecuteRequest(RequestEnvelope requestEnvelope)
        {
            _pushLogger.Debug(
                $"Invoking HTTP {requestEnvelope.Method} " +
                $"on {requestEnvelope.Url}");

            // Create HttpWebRequest
            var request = _requestFactory.Make(requestEnvelope);
            var hostname = request.RequestUri.Host;

            // Invoke the Throttler
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
    }
}


