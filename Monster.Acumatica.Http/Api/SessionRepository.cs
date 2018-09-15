using Monster.Acumatica.Config;
using Push.Foundation.Web.Http;

namespace Monster.Acumatica.Api
{
    public class SessionRepository
    {
        private readonly HttpFacade _clientFacade;
        private readonly AcumaticaHttpSettings _settings;


        public SessionRepository(
                    HttpFacade clientFacade, 
                    AcumaticaHttpSettings settings)
        {
            _clientFacade = clientFacade;
            _settings = settings;
        }
        
        public void RetrieveSession(AcumaticaCredentials credentials)
        {
            var path = $"{credentials.InstanceUrl}/entity/auth/login";
            var content = credentials.AuthenticationJson;
            var response = _clientFacade.Post(path, content);
        }
        
        public string BuildMethodUrl(
                string path, string queryString = null)
        {
            return queryString == null
                ? $"{_settings.VersionSegment}{path}"
                : $"{_settings.VersionSegment}{path}?{queryString}";
        }
    }
}
