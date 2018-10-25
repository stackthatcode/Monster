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

        public string RetreiveStockItems(DateTime? lastModified = null)
        {
            var filter = "ItemStatus eq 'Active'";
            if (lastModified.HasValue)
            {
                var restDate = lastModified.Value.ToAcumaticaRestDate();
                filter += $" LastModified gt datetimeoffset'{restDate}'";
            }

            var queryString
                = new QueryStringBuilder()
                    .Add("filter", filter)
                    .Add("$expand", "WarehouseDetails")
                    .ToString();
            var response = _httpContext.Get($"StockItem?{queryString}");
            return response.Body;
        }

        public string AddNewStockItem(string content)
        {
            var response = _httpContext.Put("StockItems", content);
            return response.Body;
        }
    }
}
