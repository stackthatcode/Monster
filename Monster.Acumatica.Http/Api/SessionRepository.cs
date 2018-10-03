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



        // TODO - add method to End Session / Logout
    }
}
