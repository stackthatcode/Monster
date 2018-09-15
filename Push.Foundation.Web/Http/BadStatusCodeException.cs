using System;
using System.Net;

namespace Push.Foundation.Web.Http
{
    public class BadStatusCodeException : Exception
    {
        public HttpStatusCode StatusCode { get; set; }

        public BadStatusCodeException(HttpStatusCode statusCode, string message) 
                    : base(message)
        {
            StatusCode = statusCode;
        }
    }
}

