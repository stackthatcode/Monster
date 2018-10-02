using Monster.Acumatica.Http;

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
    }
}
