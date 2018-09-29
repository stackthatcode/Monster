using Monster.Acumatica.Config;
using Monster.Acumatica.Http;
using Push.Foundation.Web.Http;

namespace Monster.Acumatica.Api
{
    public class SessionRepository
    {
        private readonly UrlBuilder _urlBuilder;
        private readonly AcumaticaHttpContext _httpContext;
        private readonly AcumaticaHttpConfig _httpConfig;

        public SessionRepository(
                UrlBuilder urlBuilder, 
                AcumaticaHttpContext httpContext, 
                AcumaticaHttpConfig httpConfig)
        {
            _urlBuilder = urlBuilder;
            _httpContext = httpContext;
            _httpConfig = httpConfig;
        }
        
        public void RetrieveSession(AcumaticaCredentials credentials)
        {
            var path = $"{credentials.InstanceUrl}/entity/auth/login";
            var content = credentials.AuthenticationJson;
            var response = _httpContext.Post(path, content);
        }
        
        public string BuildMethodUrl(string path, string queryString = null)
        {
            return queryString == null
                ? $"{_httpConfig.VersionSegment}{path}"
                : $"{_httpConfig.VersionSegment}{path}?{queryString}";
        }

        // TODO - add method to End Session / Logout
    }
}
