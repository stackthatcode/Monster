using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using Monster.Acumatica.Config;
using Monster.Acumatica.Http;
using Monster.Acumatica.Model;
using Monster.ConsoleApp.TestJson;
using Push.Foundation.Utilities.Json;
using Push.Foundation.Utilities.Logging;
using Push.Shopify.Api;
using Push.Shopify.Api.Order;
using Push.Shopify.Api.Payout;
using Push.Shopify.Api.Product;
using Push.Shopify.Api.Transaction;
using Push.Shopify.Config;
using Push.Shopify.HttpClient.Credentials;
using Customer = Monster.Acumatica.Model.Customer;


namespace Monster.ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            // DeserializeJson<BalanceTransactionList>("3duPayouts20180813.json");
            // DeserializeJson<TransactionList>("3duPayPalTransactions.json");
            
            // Shopify => Bridge-Over-Monsters
            //ExecuteInLifetimeScope(scope => RetrieveOrderData(scope, 554500751458));
            //ExecuteInLifetimeScope(scope => RetrieveProductData(scope, 1403130544226));
            //ExecuteInLifetimeScope(scope => RetrieveLocations(scope));
            //ExecuteInLifetimeScope(scope => Metaplay.UpdateMetadata(scope));

            // Shopify => 3D Universe 
            //ExecuteInLifetimeScope(scope => RetrievePayoutDta(scope));            
            
            // Macbook Air => Acumatica Instance
            //ExecuteInLifetimeScope(scope => RetrieveAcumaticaItemClass(scope));

            var acumaticaHarness = new AcumaticaTestbed();
            ExecuteInLifetimeScope(scope => acumaticaHarness.RetrieveAcumaticaPostingClass(scope));
            ExecuteInLifetimeScope(scope => acumaticaHarness.RetrieveAcumaticaCustomer(scope));

            Console.WriteLine("Finished - hit any key to exit...");
            Console.ReadKey();
        }


        static void DeserializeJson<T>(string inputJsonFile)
        {
            var json = TestLoader.GimmeJson(inputJsonFile);
            var deserializedObject = json.DeserializeFromJson<T>();
            var reserializedJson = deserializedObject.SerializeToJson();
            Console.WriteLine(reserializedJson);
        }
        
        static void ExecuteInLifetimeScope(Action<ILifetimeScope> action)
        {
            using (var container = ConsoleAutofac.Build(false))
            using (var scope = container.BeginLifetimeScope())
            {
                var logger = scope.Resolve<IPushLogger>();
                try
                {
                    action(scope);
                }
                catch (Exception ex)
                {
                    logger.Error(ex);
                    throw;
                }
            }
        }
    }
}

