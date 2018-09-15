using System.Collections.Generic;
using System.IO;
using System.Net;

namespace Push.Foundation.Web.Http
{
    public class HttpWebRequestProcessor
    {
        public virtual ResponseEnvelope Execute(HttpWebRequest request)
        {
            try
            {
                using (var response = (HttpWebResponse) request.GetResponse())
                {
                    if (request.ContentType == "image")
                    {
                        return ProcessImageRequest(response);
                    }
                    else
                    {
                        return ProcessContentRequest(response);
                    }
                }
            }
            catch (WebException e)
            {
                using (var webResponse = e.Response)
                {
                    var httpResponse = (HttpWebResponse) webResponse;
                    if (httpResponse == null)
                    {
                        throw;
                    }

                    return ProcessContentRequest(httpResponse);
                }
            }
        }
        
        private ResponseEnvelope ProcessContentRequest(HttpWebResponse response)
        {
            var output = new ResponseEnvelope();

            output.StatusCode = response.StatusCode;
            output.Headers = new Dictionary<string, string>();
            foreach (var key in response.Headers.AllKeys)
            {
                output.Headers[key] = response.Headers[key];
            }

            using (var data = response.GetResponseStream())
            using (var reader = new StreamReader(data))
            {
                output.Body = reader.ReadToEnd();
            }

            return output;
        }

        private ResponseEnvelope ProcessImageRequest(HttpWebResponse response)
        {
            var output = new ResponseEnvelope();

            output.StatusCode = response.StatusCode;
            output.Headers = new Dictionary<string, string>();
            foreach (var key in response.Headers.AllKeys)
            {
                output.Headers[key] = response.Headers[key];
            }

            using (var binaryReader = new BinaryReader(response.GetResponseStream()))
            using (var memoryStream = new MemoryStream())
            {
                var buffer = binaryReader.ReadBytes(1024);
                while (buffer.Length > 0)
                {
                    memoryStream.Write(buffer, 0, buffer.Length);
                    buffer = binaryReader.ReadBytes(1024);
                }

                var numberOfBytes = (int)memoryStream.Length;
                var byteData = new byte[numberOfBytes];
                memoryStream.Position = 0;
                memoryStream.Read(byteData, 0, numberOfBytes);

                output.BinaryData = byteData;
            }

            return output;
        }
    }
}

