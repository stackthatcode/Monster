using Monster.Acumatica.Config;
using Monster.Acumatica.Http;
using Push.Foundation.Web.Http;

namespace Monster.Acumatica.Api
{
    public class InventoryRepository
    {
        private readonly HttpFacade _clientFacade;
        private readonly UrlBuilder _urlBuilder;
        private readonly AcumaticaHttpSettings _settings;


        public InventoryRepository(
                    HttpFacade clientFacade,
                    UrlBuilder urlBuilder,
                    AcumaticaHttpSettings settings)
        {
            _clientFacade = clientFacade;
            _urlBuilder = urlBuilder;
            _settings = settings;
        }

        public string RetrieveItemClass()
        {
            var url = _urlBuilder.Make("ItemClass");
            var response = _clientFacade.Get(url);
            return response.Body;
        }

        public string RetrieveStockItems()
        {
            var url = _urlBuilder.Make("StockItem");
            var response = _clientFacade.Get(url);
            return response.Body;
        }

        public string RetrievePostingClasses()
        {
            var url = _urlBuilder.Make("PostingClass");
            var response = _clientFacade.Get(url);
            return response.Body;
        }        
    }
}
