using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace Push.Foundation.Web.HttpClient
{
    public class RequestFactory
    {
        private readonly ClientSettings _configuration;

        public RequestFactory(ClientSettings configuration)
        {
            _configuration = configuration;
        }


        public HttpWebRequest HttpGet(
                string url, 
                Dictionary<string, string> headers = null,
                string contentType = "application/json")
        {
            var request = FactoryWorker(url, headers);
            request.Method = "GET";
            request.ContentType = contentType;
            return request;
        }

        public HttpWebRequest HttpPost(
                string url, string content, 
                Dictionary<string, string> headers = null,
                string contentType = "application/json")
        {
            var request = FactoryWorker(url, headers);
            request.Method = "POST";

            var byteArray = Encoding.ASCII.GetBytes(content);
            request.ContentLength = byteArray.Length;
            request.ContentType = contentType;
            Stream dataStream = request.GetRequestStream();
            dataStream.Write(byteArray, 0, byteArray.Length);
            dataStream.Close();

            return request;
        }

        public HttpWebRequest HttpPut(
                string url, string content, Dictionary<string, string> headers = null)
        {
            var request = FactoryWorker(url, headers);
            request.Method = "PUT";

            var byteArray = Encoding.ASCII.GetBytes(content);
            request.ContentLength = byteArray.Length;
            request.ContentType = "application/json";

            Stream dataStream = request.GetRequestStream();
            dataStream.Write(byteArray, 0, byteArray.Length);
            dataStream.Close();

            return request;
        }

        public HttpWebRequest HttpDelete(string url, Dictionary<string, string> headers = null)
        {
            var request = FactoryWorker(url, headers);
            request.Method = "DELETE";
            return request;
        }

        private HttpWebRequest FactoryWorker(string url, Dictionary<string, string> headers)
        {
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Ssl3;
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

            var req = (HttpWebRequest)WebRequest.Create(url);
            req.Timeout = _configuration.Timeout;

            if (headers != null)
            {
                foreach (var header in headers)
                {
                    req.Headers[header.Key] = header.Value;
                }
            }

            return req;
        }
    }
}
