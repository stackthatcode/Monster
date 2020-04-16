using System;


namespace Monster.ConsoleApp
{
    class Program
    {
        // System utilities
        //
        private const string RunHangfireBackgroundOption = "1";
        private const string ViewShopifyOrderAndTaxTransfer = "2";
        private const string ViewAcumaticaTaxSnapshot = "3";

        // User management functions
        //
        private const string HydrateSecurityConfigOption = "11";
        private const string ProvisionUserAccount = "12";
        private const string ListUserAccountsOption = "13";

        // Instance registration and provisioning functions
        //
        private const string RegisterInstanceOption = "21";
        private const string ListInstancesOption = "22";
        private const string AssignInstanceToAccountOption = "23";
        private const string RevokeInstanceOption = "24";
        private const string EnableInstanceOption = "25";
        private const string DisableInstanceOption = "26";

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
                Console.WriteLine("Hit enter to continue...");
                Console.ReadLine();
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
            Console.WriteLine($"{ViewAcumaticaTaxSnapshot} - View Acumatica Tax Transfer JSON");
            Console.WriteLine();

            // User management functions
            //
            Console.WriteLine($"{HydrateSecurityConfigOption} - Hydrate Security Config");
            Console.WriteLine($"{ProvisionUserAccount} - Provision New User Account");
            Console.WriteLine($"{ListUserAccountsOption} - List all User Accounts");
            Console.WriteLine();

            // Instance registration and provisioning functions
            //
            Console.WriteLine($"{RegisterInstanceOption} - Register new Instance");
            Console.WriteLine($"{ListInstancesOption} - List all Instances");
            Console.WriteLine($"{AssignInstanceToAccountOption} - Assign Instance to User Account");
            Console.WriteLine($"{RevokeInstanceOption} - Revoke Instance from Account by Domain");
            Console.WriteLine($"{EnableInstanceOption} - Enable Instance");
            Console.WriteLine($"{DisableInstanceOption} - Disable Instance");
            Console.WriteLine();

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
            if (input == ViewAcumaticaTaxSnapshot)
                SystemUtilities.RunViewAcumaticaTaxTransfer();

            // User management functions
            //
            if (input == HydrateSecurityConfigOption)
                SystemUtilities.HydrateSecurityConfig();
            if (input == ProvisionUserAccount)
                SystemUtilities.ProvisionNewUserAccount();
            if (input == ListUserAccountsOption)
                SystemUtilities.ListAllUserAccounts();

            // Instance registration and provisioning functions
            //
            if (input == RegisterInstanceOption)
                SystemUtilities.RegisterInstance();
            if (input == ListInstancesOption)
                SystemUtilities.ListInstances();
            if (input == AssignInstanceToAccountOption)
                SystemUtilities.AssignInstance();
            if (input == RevokeInstanceOption)
                SystemUtilities.RevokeInstance();
            if (input == DisableInstanceOption)
                SystemUtilities.DisableInstance();
            if (input == EnableInstanceOption)
                SystemUtilities.EnableInstance();

            return input.Trim() != "";
        }
    }
}

