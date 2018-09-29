using Monster.Acumatica.Http;
using Push.Foundation.Web.Helpers;


namespace Monster.Acumatica.Api
{
    public class CustomerRepository
    {
        private readonly UrlBuilder _urlBuilder;
        private readonly AcumaticaHttpContext _httpContext;

        public CustomerRepository(
                UrlBuilder urlBuilder, 
                AcumaticaHttpContext httpContext)
        {
            _urlBuilder = urlBuilder;
            _httpContext = httpContext;
        }
        
        public string RetrieveCustomers()
        {
            var queryString =
                new QueryStringBuilder()
                    .Add("expand", "MainContact,BillingContact,ShippingContact")
                    .ToString();

            var url = _urlBuilder.Make("Customer");
            var response = _httpContext.Get(url);
            return response.Body;
        }

        public string RetrieveCustomer(string customerId)
        {
            var queryString =
                new QueryStringBuilder()
                    .Add("$expand", "MainContact,BillingContact,ShippingContact")
                    .ToString();

            var url = _urlBuilder.Make($"Customer/{customerId}", queryString);
            var response = _httpContext.Get(url);
            return response.Body;
        }
        
        public string AddNewCustomer(string content)
        {
            var url = _urlBuilder.Make("Customer");
            var response = _httpContext.Put(url, content);
            return response.Body;
        }        
    }
}
