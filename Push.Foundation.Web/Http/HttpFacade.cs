using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using Push.Foundation.Web.Misc;

namespace Push.Foundation.Web.Http
{
    public class HttpFacade
    {
        private readonly HttpClient _client;
        private readonly ExecutionContext _context;

        public HttpFacade(HttpClient client, ExecutionContext context)
        {
            _client = client;
            _context = context;
        }

        public virtual ResponseEnvelope Get(
            string url,
            Dictionary<string, string> headers = null,
            string contentType = "application/json; charset=utf-8")
        {
            var response =
                DurableExecutor.Do(
                    () => _client.GetAsync(url).Result, _context);

            return response
                    .ToEnvelope()
                    .ProcessStatusCodes();
        }

        public virtual ResponseEnvelope Post(string url, string content)
        {
            var httpContent
                = new StringContent(content, Encoding.UTF8, "application/json");
            
            var response =
                DurableExecutor.Do(
                    () => _client.PostAsync(url, httpContent).Result, _context);

            return response
                .ToEnvelope()
                .ProcessStatusCodes();
        }

        public virtual ResponseEnvelope Put(string url, string content)
        {
            var httpContent
                = new StringContent(content, Encoding.UTF8, "application/json");

            var response =
                DurableExecutor.Do(
                    () => _client.PutAsync(url, httpContent).Result, _context);

            return response
                .ToEnvelope()
                .ProcessStatusCodes();
        }
    }
}
