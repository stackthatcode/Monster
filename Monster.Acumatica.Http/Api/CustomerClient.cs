using System;
using Monster.Acumatica.Http;


namespace Monster.Acumatica.Api
{
    public class CustomerClient
    {
        private readonly AcumaticaHttpContext _httpContext;

        public CustomerClient(AcumaticaHttpContext httpContext)
        {
            _httpContext = httpContext;
        }
        
        public string RetrieveCustomers(DateTime? lastModified = null)
        {
            var queryString = "$expand=MainContact";
            var response = _httpContext.Get($"Customer?{queryString}");
            return response.Body;
        }

        public string RetrieveCustomer(string customerId)
        {
            var path = $"Customer/{customerId}?$expand=MainContact";
            var response = _httpContext.Get(path);
            return response.Body;
        }
        
        public string WriteCustomer(string content)
        {
            var response = _httpContext.Put("Customer", content);
            return response.Body;
        }        
    }
}
