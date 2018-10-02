using Monster.Acumatica.Config;
using Monster.Acumatica.Http;

namespace Monster.Acumatica.Api
{
    public class SessionRepository
    {
        private readonly AcumaticaHttpContext _httpContext;
        private readonly AcumaticaHttpConfig _httpConfig;

        public SessionRepository(
                AcumaticaHttpContext httpContext, 
                AcumaticaHttpConfig httpConfig)
        {
            _httpContext = httpContext;
            _httpConfig = httpConfig;
        }

        public void RetrieveSession(AcumaticaCredentials credentials)
        {
            var path = $"/entity/auth/login";
            var content = credentials.AuthenticationJson;
            var response = _httpContext.Post(path, content);
        }


        // TODO - add method to End Session / Logout
    }
}
