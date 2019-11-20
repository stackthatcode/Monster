using System;
using System.ServiceModel.Syndication;
using Monster.Acumatica.Http;
using Monster.Acumatica.Utility;
using Push.Foundation.Utilities.Http;
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


        public string RetrieveWarehouses()
        {
            var queryString =
                    new QueryStringBuilder()
                        .Add("$expand", "Locations")
                        .ToString();

            var response = _httpContext.Get($"Warehouse?{queryString}");
            return response.Body;
        }

        public string RetrieveStockItems(DateTime? lastModifiedAcuTz = null, int page = 1, int? pageSize = null)
        {
            var queryString = "$filter=ItemStatus eq 'Active'";

            if (lastModifiedAcuTz.HasValue)
            {
                var restDate = lastModifiedAcuTz.Value.ToAcumaticaRestDateEncode();
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

        public string RetrieveInventoryStatus(DateTime? lastModifiedAcuTz = null, int page = 1, int? pageSize = null)
        {
            var builder = new QueryStringBuilder().Add("$format", "json");

            if (lastModifiedAcuTz.HasValue)
            {
                builder.Add("$filter",
                    $"INSiteStatus_lastModifiedDateTime gt datetime'{lastModifiedAcuTz.Value.ToAcumaticaRestDate()}'");
            }

            if (pageSize.HasValue)
            {
                builder.AddPaging(page, pageSize.Value);
            }

            var queryString = builder.ToString();

            var response = _httpContext.Get($"/OData/InventoryStatus?{queryString}", excludeVersion:true);
            return response.Body;
        }


        public string AddInventoryReceipt(string content)
        {
            var response = _httpContext.Put("InventoryReceipt", content);
            return response.Body;
        }

        public string ReleaseInventoryReceipt(string content)
        {
            var response = _httpContext.Post("InventoryReceipt/ReleaseInventoryReceipt", content);
            return response.Body;
        }
    }
}
