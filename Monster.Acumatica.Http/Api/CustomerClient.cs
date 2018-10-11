using Monster.Acumatica.Http;
using Push.Foundation.Web.Helpers;


namespace Monster.Acumatica.Api
{
    public class CustomerClient
    {
        private readonly AcumaticaHttpContext _httpContext;

        public CustomerClient(AcumaticaHttpContext httpContext)
        {
            _httpContext = httpContext;
        }
        
        public string RetrieveCustomers()
        {
            var queryString =
                new QueryStringBuilder()
                    .Add("expand", "MainContact,BillingContact,ShippingContact")
                    .ToString();

            var response = _httpContext.Get("Customer");
            return response.Body;
        }

        public string RetrieveCustomer(string customerId)
        {
            var queryString =
                new QueryStringBuilder()
                    .Add("$expand", "MainContact,BillingContact,ShippingContact")
                    .ToString();

            var path = $"Customer/{customerId}?{queryString}";
            var response = _httpContext.Get(path);
            return response.Body;
        }
        
        public string AddNewCustomer(string content)
        {
            var response = _httpContext.Put("Customer", content);
            return response.Body;
        }        
    }
}
