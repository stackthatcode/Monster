using Push.Foundation.Utilities.Helpers;
using System.Collections.Generic;
using System.Net;

namespace Push.Foundation.Web.HttpClient
{
    public class RequestEnvelope
    {
        public string Method { get; set; }
        public string Url { get; set; }
        public string ContentType { get; set; }
        public string Content { get; set; }
        public Dictionary<string, string> Headers { get; set; }

        public RequestEnvelope(
                string method, 
                string url, 
                string content = null,
                string contentType = "text/json",
                Dictionary<string, string> headers = null)
        {
            Method = method;
            Url = url;
            ContentType = contentType;
            Content = content;
            Headers = headers;
        }

        public HttpWebRequest MakeWebRequest(string baseUrl = "")
        {
            var completePath = $"{baseUrl}{Url}";
            var request = (HttpWebRequest)WebRequest.Create(completePath);

            request.Method = Method;
            request.ContentType = ContentType;

            if (!Content.IsNullOrEmpty())
            {
                request.LoadContent(Content);
            }
            
            if (Headers != null)
            {
                foreach (var header in Headers)
                {
                    request.Headers[header.Key] = header.Value;
                }
            }

            return request;
        }
    }
}
