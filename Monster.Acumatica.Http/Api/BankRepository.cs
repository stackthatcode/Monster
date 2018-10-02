using Push.Foundation.Utilities.Logging;
using Push.Foundation.Web.Http;

namespace Monster.Acumatica.Api
{
    public class BankRepository
    {
        private readonly HttpFacade _clientFacade;
        private readonly IPushLogger _logger;


        public BankRepository(HttpFacade clientFacade, IPushLogger logger)
        {
            _clientFacade = clientFacade;
            _logger = logger;
        }
        
        public string RetrieveImportBankTransactions()
        {
            var response = _clientFacade.Get("ImportBankTransactions");
            return response.Body;
        }

        public string InsertImportBankTransaction(string content)
        {
            var response = _clientFacade.Put("ImportBankTransactions", content);
            _logger.Debug(content);
            return response.Body;
        }
    }
}

