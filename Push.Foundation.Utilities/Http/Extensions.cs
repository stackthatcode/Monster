using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;

namespace Push.Foundation.Utilities.Http
{
    public static class Extensions
    {
        public static void LoadContent(this HttpWebRequest request, string content)
        {
            var byteArray = Encoding.ASCII.GetBytes(content);
            request.ContentLength = byteArray.Length;

            var dataStream = request.GetRequestStream();
            dataStream.Write(byteArray, 0, byteArray.Length);
            dataStream.Close();

            //request.Headers["content-length"] = byteArray.Length.ToString();
        }

        public static ResponseEnvelope ToEnvelope(
                this HttpResponseMessage message, string url = null, string requestBody = null)
        {
            var output = new ResponseEnvelope();

            output.Body = message.Content.ReadAsStringAsync().Result;
            output.StatusCode = message.StatusCode;
            output.Headers = new Dictionary<string, string>();
            
            foreach (var header in message.Headers)
            {
                output.Headers[header.Key] = string.Join("", header.Value);
            }

            return output;
        }

        public static void AddBasicAuthentication(
                    this HttpWebRequest request, string username, string password)
        {
            var encoded =
                Convert.ToBase64String(
                    Encoding.GetEncoding("ISO-8859-1")
                            .GetBytes(username + ":" + password));

            request.Headers["Authorization"] = $"Basic {encoded}";
        }
    }
}
