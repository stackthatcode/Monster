﻿using System.Net;
using Push.Foundation.Web.HttpClient;

namespace Monster.Acumatica.Http
{
    public class AcumaticaRequestBuilder : IRequestBuilder
    {
        private readonly AcumaticaHttpSettings _config;
        private readonly AcumaticaCredentials _credentials;
        private readonly CookieContainer _cookies;


        // This is instanced by the ApiFactory, which passes the valid credentials
        public AcumaticaRequestBuilder(
                AcumaticaHttpSettings config,
                AcumaticaCredentials credentials)
        {
            _config = config;
            _credentials = credentials;
            _cookies = new CookieContainer();
        }
        

        public HttpWebRequest Make(RequestEnvelope requestEnvelope)
        {
            ServicePointManager.Expect100Continue = true;
            //ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

            // Spawn the WebRequest
            var req = requestEnvelope.MakeWebRequest(_credentials.InstanceUrl);
            req.Timeout = _config.Timeout;
            req.CookieContainer = _cookies;

            return req;
        }
    }
}
