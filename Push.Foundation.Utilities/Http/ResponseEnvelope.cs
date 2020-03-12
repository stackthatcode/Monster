using System.Collections.Generic;
using System.Net;

namespace Push.Foundation.Utilities.Http
{
    public class ResponseEnvelope
    {
        // Request
        //
        public string Url { get; set; }
        public string RequestBody { get; set; }
        
        // Response
        //
        public HttpStatusCode StatusCode { get; set; }
        public Dictionary<string, string> Headers { get; set; }
        public string Body { get; set; }
        public byte[] BinaryData { get; set; }

        public LinkHeader LinkHeader => Headers.ContainsKey("Link") ? Headers["Link"].ToLinkHeader() : null;

        public ResponseEnvelope()
        {
            StatusCode = HttpStatusCode.Accepted;
        }

        public bool HasBadStatusCode =>
                !(this.StatusCode == HttpStatusCode.OK
                || this.StatusCode == HttpStatusCode.NoContent
                || this.StatusCode == HttpStatusCode.Created
                || this.StatusCode == HttpStatusCode.Accepted
                || this.StatusCode == HttpStatusCode.NotFound);

        public bool HasBadStatusCodeAndNotFound =>
            !(this.StatusCode == HttpStatusCode.OK
              || this.StatusCode == HttpStatusCode.NoContent
              || this.StatusCode == HttpStatusCode.Created
              || this.StatusCode == HttpStatusCode.Accepted);

    }
}

