using Monster.Acumatica.Http;
using Push.Foundation.Utilities.Logging;

namespace Monster.Acumatica.Api
{
    public class ReferenceClient
    {
        private readonly AcumaticaHttpContext _httpContext;
        private readonly IPushLogger _logger;

        public ReferenceClient(
            IPushLogger logger, AcumaticaHttpContext httpContext)
        {
            _logger = logger;
            _httpContext = httpContext;
        }
        

        public string RetrieveItemClass()
        {
            var response = _httpContext.Get($"ItemClass");
            return response.Body;
        }
        
        public string RetrievePaymentMethod()
        {
            var response = _httpContext.Get($"PaymentMethod?$expand=AllowedCashAccounts");
            return response.Body;
        }

        public string RetrieveTaxZones()
        {
            var response = _httpContext.Get($"TaxZone");
            return response.Body;
        }

        public string RetrieveTaxCategories()
        {
            var response = _httpContext.Get($"TaxCategory");
            return response.Body;
        }

        public string RetrieveTaxes()
        {
            var response = _httpContext.Get($"Tax");
            return response.Body;
        }

        public string RetrieveCustomerClasses()
        {
            var response = _httpContext.Get($"CustomerClass");
            return response.Body;
        }

        public string RetrieveShipVia()
        {
            var response = _httpContext.Get($"ShipVia");
            return response.Body;
        }
    }
}
