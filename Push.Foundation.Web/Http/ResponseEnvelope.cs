using System.Collections.Generic;
using System.Net;


namespace Push.Foundation.Web.Http
{
    public class ResponseEnvelope
    {
        public bool IsBinary => BinaryData != null;
        public HttpStatusCode StatusCode { get; set; }
        public Dictionary<string, string> Headers { get; set; }
        public string Body { get; set; }
        public byte[] BinaryData { get; set; }


        public ResponseEnvelope()
        {
            StatusCode = HttpStatusCode.Accepted;
        }

        

        public virtual ResponseEnvelope ProcessStatusCodes()
        {
            // UPDATE - 08/22/2018 - We're squashing this for now
            // HTTP 404's are the only non-200 calls for which we won't throw exceptions
            //if (this.StatusCode == HttpStatusCode.NotFound)
            //{
            //    return this;
            //}

            // All other non-200 calls throw an exception
            if (this.StatusCode != HttpStatusCode.OK
                    && this.StatusCode != HttpStatusCode.NoContent
                    && this.StatusCode != HttpStatusCode.Created
                    && this.StatusCode != HttpStatusCode.Accepted)
            {
                throw new BadStatusCodeException(this.StatusCode, this.Body);
            }

            return this;
        }
    }
}
