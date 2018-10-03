using System;
using Monster.ConsoleApp.Payouts;


namespace Monster.ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            // *** Uncomment a process flow to execute...


            // Acumatica test runs
            //AcumaticaProcess.Execute();


            // Shopify test runs
            // ShopifyHarness.RunShopifyMetafieldCopy();
            // TODO - Product, Orders, Locations


            // Payouts runs
            PayoutsHarness.RunPayoutsByTenant();
            //PayoutsRunner.StressTestDataPopulate();


            // Monster test runs
            //var guid = Guid.NewGuid();
            //MonsterHarness.TestInventoryWorker(guid);

            Console.WriteLine("Finished - hit any key to exit...");
            Console.ReadKey();
        }
    }
}

