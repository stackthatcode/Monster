using System;
using System.IO;
using System.Net;
using System.Text;

namespace Monster.Acumatica.Http
{
    public class AcumaticaRequestBuilder
    {
        private readonly AcumaticaHttpSettings _config;       
        private readonly CookieContainer _cookies;


        // This is instanced by the ApiFactory, which passes the valid credentials
        public AcumaticaRequestBuilder(AcumaticaHttpSettings config)
        {
            _config = config;
            _cookies = new CookieContainer();
        }


        public HttpWebRequest HttpGet(string path)
        {
            var request = FactoryWorker(path);
            request.Method = "GET";
            return request;
        }

        public HttpWebRequest HttpPost(string path, string content)
        {
            var request = FactoryWorker(path);
            request.Method = "POST";

            var byteArray = Encoding.ASCII.GetBytes(content);
            request.ContentLength = byteArray.Length;
            request.ContentType = "application/json";

            Stream dataStream = request.GetRequestStream();
            dataStream.Write(byteArray, 0, byteArray.Length);
            dataStream.Close();

            return request;
        }

        public HttpWebRequest HttpPut(string path, string content)
        {
            var request = FactoryWorker(path);
            request.Method = "PUT";

            var byteArray = Encoding.ASCII.GetBytes(content);
            request.ContentLength = byteArray.Length;
            request.ContentType = "application/json";

            Stream dataStream = request.GetRequestStream();
            dataStream.Write(byteArray, 0, byteArray.Length);
            dataStream.Close();

            return request;
        }

        public HttpWebRequest HttpDelete(string path)
        {
            var request = FactoryWorker(path);
            request.Method = "DELETE";
            return request;
        }

        private HttpWebRequest FactoryWorker(string url)
        {
            ServicePointManager.Expect100Continue = true;
            //ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

            // Spawn the WebRequest
            var req = (HttpWebRequest)WebRequest.Create(url);
            req.Timeout = _config.Timeout;
            req.CookieContainer = _cookies;

            return req;
        }
    }
}

