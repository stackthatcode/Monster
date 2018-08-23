using Push.Foundation.Web.HttpClient;

namespace Monster.Acumatica.Http
{
    public class SpikeRepository
    {
        private readonly HttpFacade _clientFacade;

        public SpikeRepository(HttpFacade clientFacade)
        {
            _clientFacade = clientFacade;
        }
        
        public void RetrieveSession(AcumaticaCredentials credentials)
        {
            var path = "/entity/auth/login";
            var content = credentials.AuthenticationJson;
            var response = _clientFacade.Post(path, content);
        }

        public string RetrieveItemClass()
        {
            var url = BuildMethodUrl("ItemClass");
            var response = _clientFacade.Get(url);
            return response.Body;
        }

        public string BuildMethodUrl(string path)
        {
            return $"/entity/Default/17.200.001/{path}";
        }
    }
}
