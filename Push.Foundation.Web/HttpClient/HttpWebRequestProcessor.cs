using System;
using System.IO;
using System.Net;
using Push.Foundation.Utilities.Logging;

namespace Push.Foundation.Web.HttpClient
{
    public class HttpWebRequestProcessor
    {        

        private ResponseEnvelope ProcessImageRequest(HttpWebResponse resp)
        {
            using (var binaryReader = new BinaryReader(resp.GetResponseStream()))
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

                return new ResponseEnvelope
                {
                    StatusCode = resp.StatusCode,
                    BinaryData = byteData,
                };
            }
        }

        private ResponseEnvelope ProcessContentRequest(HttpWebResponse resp)
        {
            using (var sr = new StreamReader(resp.GetResponseStream()))
            {
                var messageResponse = sr.ReadToEnd();
                return new ResponseEnvelope
                {
                    StatusCode = resp.StatusCode,
                    Body = messageResponse,
                };
            }
        }


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

                    using (var data = webResponse.GetResponseStream())
                    using (var reader = new StreamReader(data))
                    {
                        var text = reader.ReadToEnd();
                        return new ResponseEnvelope()
                        {
                            StatusCode = httpResponse.StatusCode,
                            Body = text
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}

