﻿using System;
using Monster.Acumatica.Http;
using Monster.Acumatica.Utility;
using Push.Foundation.Utilities.Logging;

namespace Monster.Acumatica.Api
{
    public class SalesOrderClient
    {
        private readonly AcumaticaHttpContext _httpContext;
        private readonly IPushLogger _logger;
        
        public SalesOrderClient(
                IPushLogger logger, AcumaticaHttpContext httpContext)
        {
            _logger = logger;
            _httpContext = httpContext;
        }
        

        public string RetrieveSalesOrders(DateTime? lastModified = null)
        {
            var queryString = "$expand=Details,ShippingSettings";

            if (lastModified.HasValue)
            {
                var restDate = lastModified.Value.ToAcumaticaRestDate();
                queryString += $"&$filter=LastModified gt datetimeoffset'{restDate}'";
            }

            var response = _httpContext.Get($"SalesOrder?{queryString}");
            return response.Body;
        }

        public string RetrieveSalesOrderShipments(string salesOrderId)
        {
            var url = $"SalesOrder/SO/{salesOrderId}?$expand=Shipments";
            var response = _httpContext.Get(url);
            return response.Body;
        }

        public string WriteSalesOrder(string json)
        {
            var response = _httpContext.Put("SalesOrder", json);
            return response.Body;
        }
        
        public string RetrieveSalesOrderInvoice(string invoiceRefNbr)
        {
            var url = $"SalesInvoice/Invoice/{invoiceRefNbr}?$expand=Details";
            var response = _httpContext.Get(url);
            return response.Body;
        }
    }
}

