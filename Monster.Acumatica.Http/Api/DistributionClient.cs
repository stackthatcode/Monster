using System;
using Monster.Acumatica.Http;
using Monster.Acumatica.Utility;
using Push.Foundation.Web.Helpers;

namespace Monster.Acumatica.Api
{
    public class DistributionClient
    {
        private readonly AcumaticaHttpContext _httpContext;
        
        public DistributionClient(AcumaticaHttpContext httpContext)
        {
            _httpContext = httpContext;
        }


        public string RetrieveItemClass()
        {
            var response = _httpContext.Get("ItemClass");
            return response.Body;
        }
        
        public string RetrievePostingClasses()
        {
            var response = _httpContext.Get("PostingClass");
            return response.Body;
        }

        public string RetrieveWarehouses()
        {
            var queryString =
                    new QueryStringBuilder()
                        .Add("$expand", "Locations")
                        .ToString();

            var response = _httpContext.Get($"Warehouse?{queryString}");
            return response.Body;
        }

        public string AddNewWarehouse(string content)
        {
            var response = _httpContext.Put("Warehouse", content);
            return response.Body;
        }

        public string RetrieveItemWarehouses()
        {
            var response = _httpContext.Get("ItemWarehouse");
            return response.Body;
        }


        public string RetrieveStockItems(
                DateTime? lastModified = null, int page = 1, int? pageSize = null)
        {
            var queryString = "$expand=WarehouseDetails&$filter=ItemStatus eq 'Active'";
            if (lastModified.HasValue)
            {
                var restDate = lastModified.Value.ToAcumaticaRestDate();
                queryString += $" and LastModified gt datetimeoffset'{restDate}'";
            }

            if (pageSize.HasValue)
            {
                queryString += "&" + Paging.QueryStringParams(page, pageSize.Value);
            }

            var response = _httpContext.Get($"StockItem?{queryString}");
            return response.Body;
        }

        public string AddNewStockItem(string content)
        {
            var response = _httpContext.Put("StockItem", content);
            return response.Body;
        }

        public string RetreiveInventoryReceipts()
        {
            var response = _httpContext.Get("InventoryReceipt?$expand=Details");
            return response.Body;
        }

        public string AddInventoryReceipt(string content)
        {
            var response = _httpContext.Put("InventoryReceipt", content);
            return response.Body;
        }

        public string ReleaseInventoryReceipt(string content)
        {
            var response 
                = _httpContext.Post(
                    "InventoryReceipt/ReleaseInventoryReceipt", content);
            return response.Body;
        }

    }
}
