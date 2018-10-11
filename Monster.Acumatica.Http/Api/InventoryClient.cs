using System;
using System.Net;
using Monster.Acumatica.Http;
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

        public string RetreiveStockItems()
        {
            //var queryString = "$filter=ItemStatus eq 'Active' and LastModified gt datetimeoffset'" +
            var queryString = "$filter=ItemStatus eq 'Active'";
            
                               // WebUtility.UrlEncode(new DateTimeOffset(new DateTime(2016, 2, 1).AddMonths(-1)).ToString("yyyy-MM-ddTHH:mm:ss.fffK")) + "'";

            var response = _httpContext.Get($"StockItem?{queryString}");
            return response.Body;
        }
    }
}
