using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using Castle.Core.Internal;

namespace Push.Foundation.Web.HttpClient
{
   
    public class GenericRequestBuilder : IRequestBuilder
    {
        private readonly HttpSettings _configuration;

        public GenericRequestBuilder(HttpSettings configuration)
        {
            _configuration = configuration;
        }
        

        public HttpWebRequest Make(RequestEnvelope requestEnvelope)
        {
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Ssl3;
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

            var request = requestEnvelope.MakeWebRequest();
            return request;
        }
    }
}
