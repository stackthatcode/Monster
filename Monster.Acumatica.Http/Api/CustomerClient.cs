using System;
using Monster.Acumatica.Http;
using Monster.Acumatica.Utility;
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
        
        public string RetrieveCustomers(DateTime? lastModified = null)
        {
            //var queryString
            //    = "$expand=MainContact,BillingContact,ShippingContact";
            //if (lastModified.HasValue)
            //{
            //    var restDate = lastModified.Value.ToAcumaticaRestDate();
            //    queryString += $"&$filter=LastModified gt datetimeoffset'{restDate}'";
            //}

            var queryString = "";

            var response = _httpContext.Get($"Customer?{queryString}");
            return response.Body;
        }

        public string RetrieveCustomer(string customerId)
        {
            //var queryString =
            //    new QueryStringBuilder()
            //        .Add("$expand", "MainContact,BillingContact,ShippingContact")
            //        .ToString();

            var path = $"Customer/{customerId}?";
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
