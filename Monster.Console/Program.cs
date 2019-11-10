﻿using System;
using System.Configuration;
using Autofac;
using Hangfire;
using Hangfire.Logging;
using Hangfire.SqlServer;
using Monster.ConsoleApp.Testing;
using Monster.Middle;
using Monster.Middle.Config;
using Monster.Middle.Identity;
using Monster.Middle.Misc.Hangfire;
using Push.Foundation.Utilities.Helpers;


namespace Monster.ConsoleApp
{
    class Program
    {
        // System utilities
        //
        private const string RunHangfireBackgroundOption = "1";
        private const string ProvisionNewUserAccountOption = "2";
        private const string HydrateSecurityConfigOption = "3";
        private const string ViewShopifyOrderAndTaxTransfer = "4";
        private const string ViewAcumaticaTaxTransfer = "5";
        private const string ShopifyOrderGetToAcumaticaOrderPut = "6";
        private const string RunShopifyOrderFeederOption = "7";

        // Testing functions
        //
        private const string ShopifyOrderTimezoneTest = "101";
        private const string ShopifyOrderGet = "102";
        private const string AcumaticaCustomerGet = "103";
        private const string AcumaticaOrderGet = "104";
        private const string AcumaticaOrderSync = "105";
        private const string AcumaticaPaymentGet = "106";
        private const string AcumaticaSalesOrderRetrieve = "107";



        static void Main(string[] args)
        {
            Console.WriteLine($"Shopify-Acumatica Bridge Console App");
            Console.WriteLine($"++++++++++++++++++++++++++++++++++++");
            Console.WriteLine($"Logic Automated LLC - all rights reserved");
            Console.WriteLine();


            // System utility functions
            //
            Console.WriteLine();
            Console.WriteLine($"{RunHangfireBackgroundOption} - Run Hangfire Background Service");
            Console.WriteLine($"{ProvisionNewUserAccountOption} - Provision New User Account");
            Console.WriteLine($"{HydrateSecurityConfigOption} - Hydrate Security Config");
            Console.WriteLine($"{ViewShopifyOrderAndTaxTransfer} - View Shopify Order and Tax Transfer JSON");
            Console.WriteLine($"{ViewAcumaticaTaxTransfer} - View Acumatica Tax Transfer JSON");
            Console.WriteLine($"{ShopifyOrderGetToAcumaticaOrderPut} - Shopify Order Get + Acumatica Order Put");
            //Console.WriteLine($"{RunShopifyOrderFeederOption} - Run Shopify Test Order Feeder");


            // Component testing functions
            //
            Console.WriteLine();
            Console.WriteLine($"{ShopifyOrderTimezoneTest} - Shopify Order to Acumatica Timezone Test");
            Console.WriteLine($"{ShopifyOrderGet} - Shopify Order Get (Automatic)");
            Console.WriteLine($"{AcumaticaCustomerGet} - Acumatica Customer Get");
            Console.WriteLine($"{AcumaticaOrderGet} - Acumatica Order Get");
            Console.WriteLine($"{AcumaticaOrderSync} - Acumatica Order Sync (Order ID)");
            Console.WriteLine($"{AcumaticaPaymentGet} - Acumatica Payment Get");
            Console.WriteLine($"{AcumaticaSalesOrderRetrieve} - Acumatica Sales Order Retrieve");
            Console.WriteLine();

            Console.WriteLine(Environment.NewLine + "Make a selection and hit ENTER:");

            var input = Console.ReadLine();

            // Monster utility functions
            //
            if (input == RunHangfireBackgroundOption)
                SystemUtilities.RunHangFireBackgroundService();
            if (input == ProvisionNewUserAccountOption)
                SystemUtilities.ProvisionNewUserAccount();
            if (input == HydrateSecurityConfigOption)
                SystemUtilities.HydrateSecurityConfig();
            if (input == ViewShopifyOrderAndTaxTransfer)
                SystemUtilities.RunViewShopifyOrderAndTaxTransfer();
            if (input == ViewAcumaticaTaxTransfer)
                SystemUtilities.RunViewAcumaticaTaxTransfer();
            if (input == ShopifyOrderGetToAcumaticaOrderPut)
                SystemUtilities.RunShopifyOrderGetToAcumaticaOrderPut();

            //if (input == RunShopifyOrderFeederOption)
            //    MoreTestingStuff.RunShopifyOrderFeeder();


            // Testing functions
            //
            if (input == ShopifyOrderTimezoneTest)
                MoreTestingStuff.RunShopifyOrderTimezoneTest();
            if (input == ShopifyOrderGet)
                MoreTestingStuff.RunShopifyOrderGet();
            if (input == AcumaticaOrderSync)
                MoreTestingStuff.RunAcumaticaOrderSync();
            if (input == AcumaticaCustomerGet)
                MoreTestingStuff.RunAcumaticaCustomerGet();
            if (input == AcumaticaOrderGet)
                MoreTestingStuff.RunAcumaticaOrderGet();
            if (input == AcumaticaPaymentGet)
                MoreTestingStuff.RunAcumaticaPaymentGet();
            if (input == AcumaticaSalesOrderRetrieve)
                MoreTestingStuff.RunAcumaticaSalesOrderRetrieve();

            Console.WriteLine("FIN");
            Console.ReadKey();
        }


    }
}

