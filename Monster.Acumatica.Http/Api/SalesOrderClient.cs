﻿using System;
using System.Collections.Generic;
using Monster.Acumatica.Api.SalesOrder;
using Monster.Acumatica.Config;
using Monster.Acumatica.Http;
using Monster.Acumatica.Utility;
using Push.Foundation.Utilities.Http;
using Push.Foundation.Utilities.Json;
using Push.Foundation.Utilities.Logging;


namespace Monster.Acumatica.Api
{
    public class SalesOrderClient
    {
        private readonly AcumaticaHttpContext _httpContext;
        private readonly AcumaticaHttpConfig _config;
        
        public SalesOrderClient(AcumaticaHttpContext httpContext, AcumaticaHttpConfig config)
        {
            _httpContext = httpContext;
            _config = config;
        }
        

        public List<SalesOrder.SalesOrder> RetrieveUpdatedSalesOrders(
                DateTime lastModified, int page = 1, int? pageSize = null, string expand = Expand.Shipments_Totals)
        {
            var builder = new QueryStringBuilder().Add("$expand", expand);

            // Date filtering
            //
            var restDate = lastModified.ToAcumaticaRestDate();
            builder.Add("$filter", $"LastModified gt datetimeoffset'{restDate}'");

            // Paging
            //
            pageSize = pageSize ?? _config.PageSize;
            builder.AddPaging(page, pageSize.Value);
 
            // Customer Tax Snapshot field
            //
            builder.Add("$custom", "Document.UsrTaxSnapshot");

            var queryString = builder.ToString();
            var response = _httpContext.Get($"SalesOrder?{queryString}");
            return response.Body.DeserializeFromJson<List<SalesOrder.SalesOrder>>();
        }

        public List<SalesOrder.SalesOrder> FindSalesOrder(
                    string customerOrder, string expand = Expand.Shipments_Totals)
        {
            var path = $"SalesOrder/?$filter=CustomerOrder eq '{customerOrder}'&$expand={expand}";
            var response = _httpContext.Get(path);
            return response.Body.DeserializeFromJson<List<SalesOrder.SalesOrder>>();
        }

        public string RetrieveSalesOrder(
                    string orderNbr, string orderType, string expand = Expand.Shipments_Totals)
        {
            var path = $"SalesOrder/{orderType}/{orderNbr}?$expand={expand}&$custom=Document.UsrTaxSnapshot";
            var response = _httpContext.Get(path);
            return response.Body;
        }

        public string WriteSalesOrder(string json, string expand)
        {
            var response = _httpContext.Put($"SalesOrder?$expand={expand}", json);
            return response.Body;
        }
    }
}
