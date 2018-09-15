using Monster.Acumatica.Config;
using Monster.Acumatica.Http;
using Push.Foundation.Web.Helpers;
using Push.Foundation.Web.Http;

namespace Monster.Acumatica.Api
{
    public class CustomerRepository
    {
        private readonly HttpFacade _clientFacade;
        private readonly UrlBuilder _urlBuilder;
        private readonly AcumaticaHttpSettings _settings;


        public CustomerRepository(
                    HttpFacade clientFacade, 
                    UrlBuilder urlBuilder,
                    AcumaticaHttpSettings settings)
        {
            _clientFacade = clientFacade;
            _urlBuilder = urlBuilder;
            _settings = settings;
        }
        
        public string RetrieveCustomers()
        {
            var queryString =
                new QueryStringBuilder()
                    .Add("expand", "MainContact,BillingContact,ShippingContact")
                    .ToString();

            var url = _urlBuilder.Make("Customer");
            var response = _clientFacade.Get(url);
            return response.Body;
        }

        public string RetrieveCustomer(string customerId)
        {
            var queryString =
                new QueryStringBuilder()
                    .Add("$expand", "MainContact,BillingContact,ShippingContact")
                    .ToString();

            var url = _urlBuilder.Make($"Customer/{customerId}", queryString);
            var response = _clientFacade.Get(url);
            return response.Body;
        }
        
        public string AddNewCustomer(string content)
        {
            var url = _urlBuilder.Make("Customer");
            var response = _clientFacade.Put(url, content);
            return response.Body;
        }
        
    }
}
