using System;
using System.Collections.Generic;
using Monster.Acumatica.Api.Common;
using Monster.Acumatica.Api.SalesOrder;
using Monster.Acumatica.Config;
using Monster.Acumatica.Http;
using Monster.Acumatica.Utility;
using Push.Foundation.Utilities.Http;
using Push.Foundation.Utilities.Json;
using Push.Foundation.Utilities.Logging;


namespace Monster.Acumatica.Api
{
    public class InvoiceClient
    {
        private readonly AcumaticaHttpContext _httpContext;
        private readonly AcumaticaHttpConfig _config;
        private readonly IPushLogger _logger;
        
        public InvoiceClient(IPushLogger logger, AcumaticaHttpContext httpContext, AcumaticaHttpConfig config)
        {
            _logger = logger;
            _httpContext = httpContext;
            _config = config;
        }
        
        public string WriteInvoice(string json)
        {
            var response = _httpContext.Put($"Invoice", json);   
            return response.Body;
        }

        public string RetrieveInvoiceAndTaxes(string invoiceRefNbr, string invoiceType)
        {
            var url = $"Invoice/{invoiceType}/{invoiceRefNbr}?$expand=ApplicationsCreditMemo";// {Expand.ApplicationsDefault}";
            var response = _httpContext.Get(url);
            return response.Body;
        }

        public string ReleaseInvoice(string invoiceRefNbr, string invoiceType)
        {
            var payload = new
            {
                entity = new
                {
                    Type = invoiceType.ToValue(),
                    ReferenceNbr = invoiceRefNbr.ToValue(),
                }
            };

            var content = payload.SerializeToJson();
            var response = _httpContext.Post("Invoice/ReleaseInvoice", content);
            return response.Body;
        }

    }
}

