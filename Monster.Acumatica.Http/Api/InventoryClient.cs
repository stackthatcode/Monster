using System;
using System.Net;
using Monster.Acumatica.Http;
using Monster.Acumatica.Utility;
using Push.Foundation.Web.Helpers;

namespace Monster.Acumatica.Api
{
    public class InventoryClient
    {
        private readonly AcumaticaHttpContext _httpContext;
        
        public InventoryClient(AcumaticaHttpContext httpContext)
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
                        .Add("expand", "Locations")
                        .ToString();

            var response = _httpContext.Get($"Warehouse?{queryString}");
            return response.Body;
        }

        public string AddNewWarehouse(string content)
        {
            var response = _httpContext.Put("Warehouse", content);
            return response.Body;
        }

        public string AddNewStockItem(string content)
        {
            var response = _httpContext.Put("StockItems", content);
            return response.Body;
        }

        public string RetreiveStockItems(DateTime? lastModified = null)
        {               
            var queryString = "$filter=ItemStatus eq 'Active'";

            if (lastModified.HasValue)
            {
                var restDate = lastModified.Value.ToAcumaticaRestDate();
                queryString += $" LastModified gt datetimeoffset'{restDate}'";
            }

            var response = _httpContext.Get($"StockItem?{queryString}");
            return response.Body;
        }
    }
}
