using System;
using Autofac;
using Monster.ConsoleApp.Acumatica;
using Monster.ConsoleApp.TestJson;
using Push.Foundation.Utilities.Json;
using Push.Foundation.Utilities.Logging;


namespace Monster.ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            // DeserializeJson<BalanceTransactionList>("3duPayouts20180813.json");
            // DeserializeJson<TransactionList>("3duPayPalTransactions.json");

            // Shopify => Bridge-Over-Monsters
            //ExecuteInLifetimeScope(scope => ShopifyHarness.RetrievePayoutData(scope));            

            // Macbook Air => Acumatica Instance
            ExecuteInLifetimeScope(scope => AcumaticaHarness.InsertImportBankTransactions(scope));            

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

