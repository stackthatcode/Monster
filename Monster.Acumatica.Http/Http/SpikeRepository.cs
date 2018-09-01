using Push.Foundation.Web.Helpers;
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
            var queryString =
                new QueryStringBuilder()
                    .Add("expand", "MainContact,BillingContact,ShippingContact")
                    .ToString();

            var url = BuildMethodUrl("Customer");
            var response = _clientFacade.Get(url);
            return response.Body;
        }

        public string RetrieveCustomer(string customerId)
        {
            var queryString =
                new QueryStringBuilder()
                    .Add("$expand", "MainContact,BillingContact,ShippingContact")
                    .ToString();

            var url = BuildMethodUrl($"Customer/{customerId}", queryString);
            var response = _clientFacade.Get(url);
            return response.Body;
        }


        public string AddNewCustomer(string input)
        {
            var url = BuildMethodUrl("Customer");
            var response = _clientFacade.Put(url);
            return response.Body;
        }


        public void CreateCustomer()
        {
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
