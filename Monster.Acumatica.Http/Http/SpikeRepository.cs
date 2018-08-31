using Push.Foundation.Web.HttpClient;

namespace Monster.Acumatica.Http
{
    public class SpikeRepository
    {
        private readonly HttpFacade _clientFacade;
        private readonly AcumaticaHttpSettings _settings;


        public SpikeRepository(
                HttpFacade clientFacade, AcumaticaHttpSettings settings)
        {
            _clientFacade = clientFacade;
            _settings = settings;
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

        public string RetrieveStockItems()
        {
            var url = BuildMethodUrl("StockItem");
            var response = _clientFacade.Get(url);
            return response.Body;
        }

        public string RetrievePostingClasses()
        {
            var url = BuildMethodUrl("PostingClass");
            var response = _clientFacade.Get(url);
            return response.Body;
        }


        public string RetrieveCustomers()
        {
            var url = BuildMethodUrl("Customer");
            var response = _clientFacade.Get(url);
            return response.Body;
        }

        public void CreateCustomer()
        {
        }



        public string BuildMethodUrl(string path)
        {
            return $"{_settings.VersionSegment}{path}";
        }
    }
}
