using Monster.Acumatica.Http;
using Push.Foundation.Web.Helpers;

namespace Monster.Acumatica.Api
{
    public class InventoryRepository
    {
        private readonly AcumaticaHttpContext _httpContext;


        public InventoryRepository(AcumaticaHttpContext httpContext)
        {
            _httpContext = httpContext;
        }

        public string RetrieveItemClass()
        {
            var response = _httpContext.Get("ItemClass");
            return response.Body;
        }

        public string RetrieveStockItems()
        {
            var response = _httpContext.Get("StockItem");
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
    }
}
