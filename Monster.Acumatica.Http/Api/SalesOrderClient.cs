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
        
        public SalesOrderClient(IPushLogger logger, AcumaticaHttpContext httpContext)
        {
            _logger = logger;
            _httpContext = httpContext;
        }
        
        public string RetrieveSalesOrders(DateTime? lastModified = null)
        {
            var queryString = "$expand=Details";

            if (lastModified.HasValue)
            {
                var restDate = lastModified.Value.ToAcumaticaRestDate();
                queryString += $"&$filter=LastModified gt datetimeoffset'{restDate}'";
            }

            var response = _httpContext.Get($"SalesOrder?{queryString}");
            return response.Body;
        }

        public string AddSalesOrder(string json)
        {
            var response = _httpContext.Put("SalesOrder", json);
            return response.Body;
        }
    }
}
