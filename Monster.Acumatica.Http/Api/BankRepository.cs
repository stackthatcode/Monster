using Monster.Acumatica.Config;
using Monster.Acumatica.Http;
using Push.Foundation.Web.Helpers;
using Push.Foundation.Web.Http;

namespace Monster.Acumatica.Api
{
    public class BankRepository
    {
        private readonly HttpFacade _clientFacade;
        private readonly UrlBuilder _urlBuilder;
        private readonly AcumaticaHttpSettings _settings;


        public BankRepository(
                    HttpFacade clientFacade,
                    UrlBuilder urlBuilder,
                    AcumaticaHttpSettings settings)
        {
            _clientFacade = clientFacade;
            _urlBuilder = urlBuilder;
            _settings = settings;
        }
        
        public string RetrieveImportBankTransactions()
        {
            var url = _urlBuilder.Make("ImportBankTransactions");
            var response = _clientFacade.Get(url);
            return response.Body;
        }

        public string InsertImportBankTransaction(string content)
        {
            var url = _urlBuilder.Make("ImportBankTransactions");
            var response = _clientFacade.Put(url, content);
            return response.Body;
        }
    }
}

