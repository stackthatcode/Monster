using Monster.Acumatica.Http;

namespace Monster.Acumatica.Api
{
    public class InventoryRepository
    {
        private readonly UrlBuilder _urlBuilder;
        private readonly AcumaticaHttpContext _httpContext;


        public InventoryRepository(
                UrlBuilder urlBuilder, AcumaticaHttpContext httpContext)
        {
            _urlBuilder = urlBuilder;
            _httpContext = httpContext;
        }

        public string RetrieveItemClass()
        {
            var url = _urlBuilder.Make("ItemClass");
            var response = _httpContext.Get(url);
            return response.Body;
        }

        public string RetrieveStockItems()
        {
            var url = _urlBuilder.Make("StockItem");
            var response = _httpContext.Get(url);
            return response.Body;
        }

        public string RetrievePostingClasses()
        {
            var url = _urlBuilder.Make("PostingClass");
            var response = _httpContext.Get(url);
            return response.Body;
        }        
    }
}
