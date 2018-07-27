using System;
using System.Net;
using System.Text;

namespace Push.Foundation.Web.HttpClient
{
    public static class Extensions
    {
        public static HttpWebResponse GetResponseNoException(this HttpWebRequest req)
        {
            try
            {
                return (HttpWebResponse)req.GetResponse();
            }
            catch (WebException we)
            {
                var resp = we.Response as HttpWebResponse;
                if (resp == null)
                    throw;
                return resp;
            }
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
