using System;
using Monster.Acumatica.Config;
using Monster.Acumatica.Http;
using Monster.Acumatica.Utility;

namespace Monster.Acumatica.Api
{
    public class ShipmentClient
    {
        private readonly AcumaticaHttpContext _httpContext;
        private readonly AcumaticaHttpConfig _config;

        public ShipmentClient(AcumaticaHttpContext httpContext, AcumaticaHttpConfig config)
        {
            _httpContext = httpContext;
            _config = config;
        }


        public string RetrieveShipments(DateTime minLastModified, 
                string expand = "Details", int page = 1, int? pageSize = null)
        {
            var queryString = $"$expand={expand}";

            // Date filtering
            //
            var restDate = minLastModified.ToAcumaticaRestDate();
            queryString += $"&$filter=LastModifiedDateTime gt datetimeoffset'{restDate}'";

            // Paging
            //
            pageSize = pageSize ?? _config.PageSize;
            queryString += "&" + Paging.QueryStringParams(page, pageSize.Value);

            var response = _httpContext.Get($"Shipment?{queryString}");
            return response.Body;
        }

        public string RetrieveShipment(string shipmentNbr)
        {
            var queryString = "$expand=Details,Packages";
            var response = _httpContext.Get($"Shipment/{shipmentNbr}?{queryString}");
            return response.Body;
        }
    }
}

