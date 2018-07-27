using System.IO;
using System.Net;
using Push.Foundation.Utilities.Logging;

namespace Push.Foundation.Web.HttpClient
{
    public class HttpWebRequestProcessor
    {
        private readonly IPushLogger _logger;

        public HttpWebRequestProcessor(IPushLogger logger)
        {
            _logger = logger;
        }

        public HttpWebRequestProcessor()
        {
            _logger = new ConsoleAndDebugLogger();
        }


        public virtual ResponseEnvelope Execute(HttpWebRequest request)
        {
            try
            {
                using (HttpWebResponse resp = (HttpWebResponse) request.GetResponse())
                {
                    if (request.ContentType == "image")
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

                            var numberOfBytes = (int) memoryStream.Length;
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
                    else
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
                }
            }
            catch (WebException e)
            {
                using (WebResponse response = e.Response)
                {
                    HttpWebResponse httpResponse = (HttpWebResponse) response;

                    if (httpResponse != null)
                    {
                        _logger.Error($"Error code: {httpResponse.StatusCode}");
                    }
                    else
                    {
                        _logger.Error(e);
                    }

                    using (Stream data = response.GetResponseStream())
                    using (var reader = new StreamReader(data))
                    {
                        string text = reader.ReadToEnd();
                        _logger.Error(text);

                        return new ResponseEnvelope()
                        {
                            StatusCode = httpResponse.StatusCode,
                            Body = text
                        };
                    }
                }
            }
        }
    }
}

