using System.Net;

namespace Push.Foundation.Web.HttpClient
{
    public interface IRequestBuilder
    {
        HttpWebRequest Make(RequestEnvelope requestEnvelope);
    }

}
