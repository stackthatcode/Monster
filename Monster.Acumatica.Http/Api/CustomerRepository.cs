﻿using Monster.Acumatica.Config;
using Push.Foundation.Web.Helpers;
using Push.Foundation.Web.Http;

namespace Monster.Acumatica.Api
{
    public class CustomerRepository
    {
        private readonly HttpFacade _clientFacade;
        private readonly AcumaticaHttpSettings _settings;


        public CustomerRepository(
            HttpFacade clientFacade, AcumaticaHttpSettings settings)
        {
            _clientFacade = clientFacade;
            _settings = settings;
        }
        
        public void RetrieveSession(AcumaticaCredentials credentials)
        {
            var path = $"{credentials.InstanceUrl}/entity/auth/login";
            var content = credentials.AuthenticationJson;
            var response = _clientFacade.Post(path, content);
        }

        public string RetrieveItemClass()
        {
            var url = BuildMethodUrl("ItemClass");
            var response = _clientFacade.Get(url);
            return response.Body;
        }

        public string RetrieveStockItems()
        {
            var url = BuildMethodUrl("StockItem");
            var response = _clientFacade.Get(url);
            return response.Body;
        }

        public string RetrievePostingClasses()
        {
            var url = BuildMethodUrl("PostingClass");
            var response = _clientFacade.Get(url);
            return response.Body;
        }
        
        public string RetrieveCustomers()
        {
            var queryString =
                new QueryStringBuilder()
                    .Add("expand", "MainContact,BillingContact,ShippingContact")
                    .ToString();

            var url = BuildMethodUrl("Customer");
            var response = _clientFacade.Get(url);
            return response.Body;
        }

        public string RetrieveCustomer(string customerId)
        {
            var queryString =
                new QueryStringBuilder()
                    .Add("$expand", "MainContact,BillingContact,ShippingContact")
                    .ToString();

            var url = BuildMethodUrl($"Customer/{customerId}", queryString);
            var response = _clientFacade.Get(url);
            return response.Body;
        }


        public string AddNewCustomer(string content)
        {
            var url = BuildMethodUrl("Customer");
            var response = _clientFacade.Put(url, content);
            return response.Body;
        }

        
        public string RetrieveImportBankTransactions()
        {
            var url = BuildMethodUrl("ImportBankTransactions");
            var response = _clientFacade.Get(url);
            return response.Body;
        }

        public string InsertImportBankTransaction(string content)
        {
            var url = BuildMethodUrl("ImportBankTransactions");
            var response = _clientFacade.Put(url, content);
            return response.Body;
        }


        public string BuildMethodUrl(
                string path, string queryString = null)
        {
            return queryString == null
                ? $"{_settings.VersionSegment}{path}"
                : $"{_settings.VersionSegment}{path}?{queryString}";
        }
    }
}
