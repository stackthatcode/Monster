using System;
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
        private const string RunHangfireBackgroundOption = "1";
        private const string ProvisionNewUserAccountOption = "2";
        private const string HydrateSecurityConfigOption = "3";

        private const string RunShopifyOrderFeederOption = "10";
        private const string ShopifyOrderTimezoneTest = "11";
        private const string ShopifyOrderGet = "12";
        private const string ShopifyOrderGetSingle = "13";

        private const string AcumaticaCustomerGet = "20";
        private const string AcumaticaOrderGet = "21";
        private const string AcumaticaOrderSync = "22";
        private const string AcumaticaPaymentGet = "23";
        private const string AcumaticaSalesOrderRetrieve = "24";

        private const string ShopifyOrderGetToAcumaticaOrderPut = "30";


        static void Main(string[] args)
        {
            Console.WriteLine($"Bridge Console App");
            Console.WriteLine($"++++++++++++++++++");
            Console.WriteLine($"Logic Automated LLC - all rights reserved");
            Console.WriteLine();
            Console.WriteLine($"{RunHangfireBackgroundOption} - Run Hangfire Background Service");
            Console.WriteLine($"{ProvisionNewUserAccountOption} - Provision New User Account");
            Console.WriteLine($"{HydrateSecurityConfigOption} - Hydrate Security Config");

            // Testing functions
            //
            Console.WriteLine();
            Console.WriteLine($"{RunShopifyOrderFeederOption} - Run Shopify Test Order Feeder");
            Console.WriteLine($"{ShopifyOrderTimezoneTest} - Shopify Order to Acumatica Timezone Test");
            Console.WriteLine($"{ShopifyOrderGet} - Shopify Order Get (Automatic)");
            Console.WriteLine($"{ShopifyOrderGetSingle} - Shopify Order Get (Shopify Order ID)");

            Console.WriteLine();
            Console.WriteLine($"{AcumaticaCustomerGet} - Acumatica Customer Get");
            Console.WriteLine($"{AcumaticaOrderGet} - Acumatica Order Get");
            Console.WriteLine($"{AcumaticaOrderSync} - Acumatica Order Sync (Order ID)");
            Console.WriteLine($"{AcumaticaPaymentGet} - Acumatica Payment Get");
            Console.WriteLine($"{AcumaticaSalesOrderRetrieve} - Acumatica Sales Order Retrieve");

            Console.WriteLine();
            Console.WriteLine($"{ShopifyOrderGetToAcumaticaOrderPut} - Shopify Order Get to Acumatica Order Put");


            Console.WriteLine(Environment.NewLine + "Make a selection and hit ENTER:");

            var input = Console.ReadLine();

            // Monster utility functions
            //
            if (input == RunHangfireBackgroundOption)
                RunHangFireBackgroundService();
            if (input == ProvisionNewUserAccountOption)
                ProvisionNewUserAccount();
            if (input == HydrateSecurityConfigOption)
                HydrateSecurityConfig();

            // Acumatica stuff
            //
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

            // Shopify stuff
            //
            if (input == RunShopifyOrderFeederOption)
                MoreTestingStuff.RunShopifyOrderFeeder();
            if (input == ShopifyOrderTimezoneTest)
                MoreTestingStuff.RunShopifyOrderTimezoneTest();
            if (input == ShopifyOrderGet)
                MoreTestingStuff.RunShopifyOrderGet();
            if (input == ShopifyOrderGetSingle)
                MoreTestingStuff.RunShopifyOrderGetById();

            // Shopify Order Get to Acumatica Order Put
            //
            if (input == ShopifyOrderGetToAcumaticaOrderPut)
                MoreTestingStuff.ShopifyOrderGetToAcumaticaOrderPut();

            Console.WriteLine("FIN");
            Console.ReadKey();
        }


        static void RunHangFireBackgroundService()
        {            
            ConfigureHangFire();

            var options = new BackgroundJobServerOptions()
                {
                    SchedulePollingInterval = new TimeSpan(0, 0, 0, 1),
                };

            options.WorkerCount =
                ConfigurationManager.AppSettings["HangFireWorkerCount"].ToIntegerAlt(10);

            using (var server = new BackgroundJobServer(options))
            {
                Console.WriteLine("Hangfire Server started. Press any key to exit...");
                Console.ReadKey();
            }
        }

        static IContainer ConfigureHangFire()
        {
            var builder = new ContainerBuilder();
            MiddleAutofac.Build(builder);

            // Wire in the Autofac container
            var container = builder.Build();
            GlobalConfiguration.Configuration.UseAutofacActivator(container);

            // Set the HangFire storage
            var systemDbConnection = MonsterConfig.Settings.SystemDatabaseConnection;
            JobStorage.Current = new SqlServerStorage(systemDbConnection);

            // HangFireLogProvider -> HangFireLogger -> LoggerSingleton
            LogProvider.SetCurrentLogProvider(new HangfireLogProvider());

            return container;
        }

        static void ProvisionNewUserAccount()
        {
            Console.WriteLine(Environment.NewLine + 
                              "Enter New User's Email Address (which will be used as User ID)");
            var email = Console.ReadLine();

            Console.WriteLine(Environment.NewLine + "Enter New User's Shopify Domain");
            var domain = Console.ReadLine();

            Console.WriteLine(Environment.NewLine + 
                              $"New Account User ID: {email} - Shopify Domain: {domain}" + 
                              Environment.NewLine + 
                              "Please type 'YES' to proceed with provisioning");
            var response = Console.ReadLine();
            if (response.ToUpper().Trim() != "YES")
            {
                return;
            }

            Action<ILifetimeScope> process = scope =>
            {
                var identityService = scope.Resolve<IdentityService>();
                var user = identityService.ProvisionNewAccount(email, domain).Result;
            };

            AutofacRunner.RunInLifetimeScope(process);
        }

        static void HydrateSecurityConfig()
        {
            AutofacRunner.RunInLifetimeScope(scope =>
            {
                var identityService = scope.Resolve<IdentityService>();
                identityService.PopulateRolesAndAdmin();
            });
        }
    }
}

