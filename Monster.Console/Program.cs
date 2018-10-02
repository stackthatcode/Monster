﻿using System;
using Monster.ConsoleApp.Acumatica;
using Monster.ConsoleApp.Shopify;


namespace Monster.ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            // *** Uncomment a process flow to execute...

            // Payouts runs
            //PayoutsRunner.RunPayoutsWithInjectionOfSettings();
            //PayoutsRunner.StressTestDataPopulate();


            // Acumatica test runs
            AcumaticaProcess.Execute();


            // Shopify test runs
            // ShopifyHarness.RunShopifyMetafieldCopy();
            // TODO - Product, Orders, Locations


            // Monster test runs
            //var guid = Guid.NewGuid();
            //MonsterHarness.TestInventoryWorker(guid);

            Console.WriteLine("Finished - hit any key to exit...");
            Console.ReadKey();
        }
    }
}

