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


        public string RetrieveShipments(DateTime? lastModified = null)
        {
            var queryString = "$expand=Details";
            if (lastModified.HasValue)
            {
                var restDate = lastModified.Value.ToAcumaticaRestDate();
                queryString += $"&$filter=LastModifiedDateTime gt datetimeoffset'{restDate}'";
            }

            var response = _httpContext.Get($"Shipment?{queryString}");
            return response.Body;
        }


        public string AddShipment(string json)
        {
            var response = _httpContext.Put("Shipment", json);
            return response.Body;
        }
    }
}

