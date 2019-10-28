using System;
using System.Collections.Generic;
using Monster.Acumatica.Api.SalesOrder;
using Monster.Acumatica.Config;
using Monster.Acumatica.Http;
using Monster.Acumatica.Utility;
using Push.Foundation.Utilities.Json;
using Push.Foundation.Utilities.Logging;

namespace Monster.Acumatica.Api
{
    public class SalesOrderClient
    {
        private readonly AcumaticaHttpContext _httpContext;
        private readonly AcumaticaHttpConfig _config;
        private readonly IPushLogger _logger;
        
        public SalesOrderClient(
                IPushLogger logger, AcumaticaHttpContext httpContext, AcumaticaHttpConfig config)
        {
            _logger = logger;
            _httpContext = httpContext;
            _config = config;
        }
        
        public List<SalesOrder.SalesOrder> 
                RetrieveUpdatedSalesOrders(
                        DateTime lastModified, int page = 1, int? pageSize = null,
                        string expand = "$expand=Shipments,ShippingSettings")
        {
            var queryString = expand;

            // Date filtering
            //
            var restDate = lastModified.ToAcumaticaRestDate();
            queryString += $"&$filter=LastModified gt datetimeoffset'{restDate}'";

            // Paging
            //
            pageSize = pageSize ?? _config.PageSize;
            queryString += "&" + Paging.QueryStringParams(page, pageSize.Value);

            // Customer Tax Snapshot field
            //
            queryString += "&$custom=Document.UsrTaxSnapshot";

            var response = _httpContext.Get($"SalesOrder?{queryString}");
            return response.Body.DeserializeFromJson<List<SalesOrder.SalesOrder>>();
        }
        
        public string RetrieveSalesOrderShipments(string salesOrderId)
        {
            var url = $"SalesOrder/SO/{salesOrderId}?$expand=Shipments";
            var response = _httpContext.Get(url);
            return response.Body;
        }

        public string RetrieveSalesOrderDetails(string salesOrderId)
        {
            var url = $"SalesOrder/SO/{salesOrderId}?$expand=Details,ShippingSettings";
            var response = _httpContext.Get(url);
            LogSalesOrderDetailIds(response.Body);
            return response.Body;
        }

        public string RetrieveSalesOrder(
                    string orderNbr, string orderType, string expand = "Shipments")
        {
            var path = $"SalesOrder/{orderType}/{orderNbr}?$expand={expand}";
            var response = _httpContext.Get(path);
            return response.Body;
        }

        public string WriteSalesOrder(string json)
        {
            var response = _httpContext.Put("SalesOrder?$custom=Document.UsrTaxSnapshot", json);
            LogSalesOrderDetailIds(response.Body);
            return response.Body;
        }

        private void LogSalesOrderDetailIds(string resultJson)
        {
            var order = resultJson.DeserializeFromJson<SalesOrder.SalesOrder>();
            var details = order.Details ?? new List<SalesOrderDetail>();
            _logger.Trace($"Sales Order {order.OrderNbr.value} Detail");

            if (details.Count == 0)
            {
                _logger.Trace("(No details returned)");
                return;
            }

            foreach (var detail in details)
            {
                _logger.Trace(
                    $"{detail.InventoryID.value} - {detail.id}" + $" - OrderQty {detail.OrderQty.value}");
            }
        }
        
        public string RetrieveSalesOrderInvoice(string invoiceRefNbr, string invoiceType)
        {
            var url = $"SalesInvoice/{invoiceType}/{invoiceRefNbr}?$expand=Details";
            var response = _httpContext.Get(url);
            return response.Body;
        }
    }
}
