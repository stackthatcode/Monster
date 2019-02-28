using System;
using Monster.Acumatica.Http;
using Monster.Acumatica.Utility;
using Push.Foundation.Utilities.Logging;


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

            if (lastModified.HasValue)
            {
                var restDate = lastModified.Value.ToAcumaticaRestDate();
                queryString += $"&$filter=LastModifiedDateTime gt datetimeoffset'{restDate}'";
            }

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

