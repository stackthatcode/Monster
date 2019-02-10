using System;
using System.Collections.Generic;
using Monster.Acumatica.Api.SalesOrder;
using Monster.Acumatica.Http;
using Monster.Acumatica.Utility;
using Push.Foundation.Utilities.Json;
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


        public string OrderInterfaceUrlById(string salesOrderId)
        {
            return 
                $"{_httpContext.BaseAddress}Main" +
                $"?ScreenId=SO301000&OrderType=SO&OrderNbr={salesOrderId}";
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

        public string RetrieveSalesOrderDetails(string salesOrderId)
        {
            var url = $"SalesOrder/SO/{salesOrderId}?$expand=Details,ShippingSettings";
            var response = _httpContext.Get(url);
            LogSalesOrderDetailIds(response.Body);
            return response.Body;
        }

        public string RetrieveSalesOrder(string salesOrderId)
        {
            var path = $"SalesOrder/{salesOrderId}?$expand=Shipments";
            var response = _httpContext.Get(path);
            return response.Body;
        }

        public string WriteSalesOrder(string json)
        {
            var response = _httpContext.Put("SalesOrder", json);
            LogSalesOrderDetailIds(response.Body);
            return response.Body;
        }

        public string PrepareSalesOrderInvoice(string json)
        {
            var response = 
                _httpContext.Post("SalesOrder/PrepareInvoice", json);

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
                    $"{detail.InventoryID.value} - {detail.id}" + 
                    $" - OrderQty {detail.OrderQty.value}");
            }
        }

        public string RetrieveSalesOrderInvoice(string invoiceRefNbr)
        {
            var url = $"SalesInvoice/Invoice/{invoiceRefNbr}?$expand=Details";
            var response = _httpContext.Get(url);
            return response.Body;
        }

        public string RetrieveInvoices()
        {
            var url = $"Invoice";
            var response = _httpContext.Get(url);
            return response.Body;
        }
    }
}

