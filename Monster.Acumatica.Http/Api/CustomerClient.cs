using System;
using Monster.Acumatica.Config;
using Monster.Acumatica.Http;
using Monster.Acumatica.Utility;
using Push.Foundation.Utilities.Json;
using Push.Foundation.Utilities.Logging;


namespace Monster.Acumatica.Api
{
    public class CustomerClient
    {
        private readonly AcumaticaHttpContext _httpContext;
        private readonly AcumaticaHttpConfig _config;

        public CustomerClient(AcumaticaHttpContext httpContext, AcumaticaHttpConfig config)
        {
            _httpContext = httpContext;
            _config = config;
        }
        

        public string RetrieveCustomers(
                DateTime lastModified, int page = 1, int? pageSize = null)
        {
            var queryString = "$expand=MainContact";

            // Date filtering
            //
            var restDate = lastModified.ToAcumaticaRestDateEncode();
            queryString += $"&$filter=LastModifiedDateTime gt datetimeoffset'{restDate}'";

            // Paging
            //
            pageSize = pageSize ?? _config.PageSize;
            queryString += "&" + Paging.QueryStringParams(page, pageSize.Value);

            var response = _httpContext.Get($"Customer?{queryString}");
            return response.Body;
        }

        public string RetrieveCustomer(string customerId)
        {
            var path = $"Customer/{customerId}?$expand=MainContact";
            var response = _httpContext.Get(path);
            return response.Body;
        }
        
        public Customer.Customer WriteCustomer(Customer.Customer customer)
        {
            var content = customer.SerializeToJson();
            var response = _httpContext.Put("Customer", content);
            return response.Body.DeserializeFromJson<Customer.Customer>();
        }        
    }
}

