using Push.Foundation.Utilities.Http;
using Push.Foundation.Utilities.Logging;

namespace Monster.Acumatica.Api
{
    public class BankClient
    {
        private readonly HttpFacade _clientFacade;
        private readonly IPushLogger _logger;


        public BankClient(HttpFacade clientFacade, IPushLogger logger)
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

