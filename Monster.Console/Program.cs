using System;
using Monster.ConsoleApp.Monster;


namespace Monster.ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {            
            // Monster test runs
            var tenantId = Guid.Parse("51AA413D-E679-4F38-BA47-68129B3F9212");

            MonsterHarness.InitialLoad(tenantId);
            MonsterHarness.RoutineExecution(tenantId);

            Console.WriteLine("Finished - hit any key to exit...");
            Console.ReadKey();
        }

        static void RunTestSequences()
        {
            // *** Uncomment a process flow to execute...            
            // Acumatica test runs
            //AcumaticaProcess.Execute();

            // Shopify test runs
            // ShopifyHarness.RunShopifyMetafieldCopy();
            // TODO - Product, Orders, Locations

            // Payouts runs
            //PayoutsHarness.RunPayoutsByTenant();
            //PayoutsRunner.StressTestDataPopulate();
        }
    }
}

