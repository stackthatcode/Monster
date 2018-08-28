using System.Net;

namespace Push.Foundation.Web.HttpClient
{
   
    public class GenericRequestBuilder : IRequestBuilder
    {
        private readonly HttpSettings _configuration;

        public GenericRequestBuilder(HttpSettings configuration)
        {
            _configuration = configuration;
        }
        

        public virtual HttpWebRequest Make(RequestEnvelope requestEnvelope)
        {
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Ssl3;
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

            var request = requestEnvelope.MakeWebRequest();
            return request;
        }
    }
}
