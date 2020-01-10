using System;
using Monster.ConsoleApp.Testing;


namespace Monster.ConsoleApp
{
    class Program
    {
        // System utilities
        //
        private const string RunHangfireBackgroundOption = "1";
        private const string ViewShopifyOrderAndTaxTransfer = "2";
        private const string ViewAcumaticaTaxTransfer = "3";
        private const string ShopifyOrderGetToAcumaticaOrderPut = "4";

        // Provisioning and user management functions
        //
        private const string HydrateSecurityConfigOption = "11";
        private const string RegisterInstanceOption = "12";
        private const string ListInstancesOption = "13";
        private const string ProvisionNewUserAccountOption = "14";
        private const string AssignInstanceToAccountOption = "15";
        private const string RevokeInstanceOption = "16";
        private const string EnableInstanceOption = "17";
        private const string DisableInstanceOption = "18";

        // Testing functions
        //
        private const string TestingFunctionsOption = "100";



        static void Main(string[] args)
        {
            Console.WriteLine($"Shopify-Acumatica Bridge Console App");
            Console.WriteLine($"++++++++++++++++++++++++++++++++++++");
            Console.WriteLine($"Logic Automated LLC - all rights reserved");
            Console.WriteLine();

            while (MainLoop())
            {
                Console.WriteLine("Hit any key to proceed...");
                Console.ReadKey();
            }

            Console.WriteLine("FIN");
            Console.ReadKey();
        }

        private static bool MainLoop()
        {
            // Run-time utility functions
            //
            Console.WriteLine();
            Console.WriteLine($"{RunHangfireBackgroundOption} - Run Hangfire Background Service");
            Console.WriteLine($"{ViewShopifyOrderAndTaxTransfer} - View Shopify Order and Tax Transfer JSON");
            Console.WriteLine($"{ViewAcumaticaTaxTransfer} - View Acumatica Tax Transfer JSON");
            Console.WriteLine($"{ShopifyOrderGetToAcumaticaOrderPut} - Shopify Order Get + Acumatica Order Put");
            //Console.WriteLine($"{RunShopifyOrderFeederOption} - Run Shopify Test Order Feeder");

            // Provisioning and user management functions
            //
            Console.WriteLine();
            Console.WriteLine($"{HydrateSecurityConfigOption} - Hydrate Security Config");
            Console.WriteLine($"{RegisterInstanceOption} - Register a new Instance database");
            Console.WriteLine($"{ListInstancesOption} - List all Instances");
            Console.WriteLine($"{ProvisionNewUserAccountOption} - Provision New User Account");
            Console.WriteLine($"{AssignInstanceToAccountOption} - Assign Instance to Account");
            Console.WriteLine($"{RevokeInstanceOption} - Revoke Instance by Domain");
            Console.WriteLine($"{EnableInstanceOption} - Enable Instance");
            Console.WriteLine($"{DisableInstanceOption} - Disable Instance");

            // Display the testing functions
            //
            Console.WriteLine();
            Console.WriteLine($"{TestingFunctionsOption} - Testing Functions");

            // Solicit input
            //
            Console.WriteLine(Environment.NewLine + "Make a selection and hit ENTER:");
            var input = Console.ReadLine();

            // Monster utility functions
            //
            if (input == RunHangfireBackgroundOption)
                SystemUtilities.RunHangFireBackgroundService();
            if (input == ViewShopifyOrderAndTaxTransfer)
                SystemUtilities.RunViewShopifyOrderAndTaxTransfer();
            if (input == ViewAcumaticaTaxTransfer)
                SystemUtilities.RunViewAcumaticaTaxTransfer();
            if (input == ShopifyOrderGetToAcumaticaOrderPut)
                SystemUtilities.RunShopifyOrderGetToAcumaticaOrderPut();

            // Provisioning and user management functions
            //
            if (input == HydrateSecurityConfigOption)
                SystemUtilities.HydrateSecurityConfig();
            if (input == RegisterInstanceOption)
                SystemUtilities.RegisterInstance();
            if (input == ProvisionNewUserAccountOption)
                SystemUtilities.ProvisionNewUserAccount();
            if (input == AssignInstanceToAccountOption)
                SystemUtilities.AssignInstance();
            if (input == RevokeInstanceOption)
                SystemUtilities.RevokeInstance();
            if (input == ListInstancesOption)
                SystemUtilities.ListInstances();
            if (input == DisableInstanceOption)
                SystemUtilities.DisableInstance();
            if (input == EnableInstanceOption)
                SystemUtilities.EnableInstance();

            // Testing functions
            //
            if (input == TestingFunctionsOption)
                DisplayTestingFunctions();

            return input.Trim() != "";
        }


        // Testing functions
        //
        private const string ShopifyOrderTimezoneTest = "101";
        private const string ShopifyOrderGet = "102";
        private const string AcumaticaCustomerGet = "103";
        private const string AcumaticaOrderGet = "104";
        private const string AcumaticaOrderSync = "105";
        private const string AcumaticaPaymentGet = "106";
        private const string AcumaticaSalesOrderRetrieve = "107";
        private const string ShopifyFulfillmentPut = "108";
        private const string ShopifyFulfillmentEmail = "109";

        private static void DisplayTestingFunctions()
        {
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
            Console.WriteLine($"{ShopifyFulfillmentPut} - Shopify Fulfillment Put");
            Console.WriteLine($"{ShopifyFulfillmentEmail} - Shopify Fulfillment Email");
            Console.WriteLine();

            Console.WriteLine(Environment.NewLine + "Make a selection and hit ENTER:");
            var input = Console.ReadLine();

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
            if (input == ShopifyFulfillmentPut)
                MoreTestingStuff.RunShopifyFulfillmentPut();
            if (input == ShopifyFulfillmentEmail)
                MoreTestingStuff.RunShopifyFulfillmentEmail();
        }
    }
}

