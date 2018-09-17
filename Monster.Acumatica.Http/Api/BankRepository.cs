using Monster.Acumatica.Config;
using Monster.Acumatica.Http;
using Push.Foundation.Utilities.Logging;
using Push.Foundation.Web.Http;

namespace Monster.Acumatica.Api
{
    public class BankRepository
    {
        private readonly HttpFacade _clientFacade;
        private readonly UrlBuilder _urlBuilder;
        private readonly AcumaticaHttpSettings _settings;
        private readonly IPushLogger _logger;


        public BankRepository(
                    HttpFacade clientFacade,
                    UrlBuilder urlBuilder,
                    AcumaticaHttpSettings settings, 
                    IPushLogger logger)
        {
            _clientFacade = clientFacade;
            _urlBuilder = urlBuilder;
            _settings = settings;
            _logger = logger;
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
            _logger.Debug(content);
            return response.Body;
        }
    }
}

