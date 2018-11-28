using System;
using Monster.ConsoleApp.Monster;


namespace Monster.ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {   
            Console.WriteLine("Monster v1.0 Testing Harness");

            // Monster test runs
            var tenantId = Guid.Parse("51AA413D-E679-4F38-BA47-68129B3F9212");

            MonsterHarness.ResetBatchStates(tenantId);

            MonsterHarness.LoadWarehouses(tenantId);
            MonsterHarness.LoadInventory(tenantId);

            MonsterHarness.RoutineShopifyPull(tenantId);
            MonsterHarness.RoutineAcumaticaPull(tenantId);
            MonsterHarness.RoutineSynchronization(tenantId);

            Console.WriteLine("Finished - hit any key to exit...");
            Console.ReadKey();
        }
    }
}

