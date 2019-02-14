using System;
using Monster.Acumatica.Http;
using Monster.Acumatica.Utility;
using Push.Foundation.Utilities.Logging;

namespace Monster.Acumatica.Api
{
    public class ShipmentClient
    {
        private readonly AcumaticaHttpContext _httpContext;
        private readonly IPushLogger _logger;

        public ShipmentClient(IPushLogger logger, AcumaticaHttpContext httpContext)
        {
            _logger = logger;
            _httpContext = httpContext;
        }


        public string RetrieveShipments(DateTime? lastModified = null, string expand = "Details")
        {
            var queryString = $"$expand={expand}";
            if (lastModified.HasValue)
            {
                var restDate = lastModified.Value.ToAcumaticaRestDate();
                queryString += $"&$filter=LastModifiedDateTime gt datetimeoffset'{restDate}'";
            }

            var response = _httpContext.Get($"Shipment?{queryString}");
            return response.Body;
        }

        public string ShipmentUrl(string shipmentNbr)
        {
            return
                $"{_httpContext.BaseAddress}" +
                $"/Main?ScreenId=SO301000&OrderType=SO&ShipmentNbr={shipmentNbr}";
        }

        public string ShipmentInvoiceUrl(string invoiceNbr)
        {
            return
                $"{_httpContext.BaseAddress}" +
                $"/Main?ScreenId=SO303000&DocType=INV&RefNbr={invoiceNbr}";
        }


        public string RetrieveShipment(string shipmentNbr)
        {
            var queryString = "$expand=Details";
            var response = 
                    _httpContext.Get($"Shipment/{shipmentNbr}?{queryString}");
            return response.Body;
        }

        public string WriteShipment(string json)
        {
            var response = _httpContext.Put("Shipment", json);
            return response.Body;
        }

        public string ConfirmShipment(string json)
        {
            var response = _httpContext.Post("Shipment/ConfirmShipment", json);
            return response.Body;
        }
    }
}

