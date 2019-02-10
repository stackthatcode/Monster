using System;
using System.Configuration;
using Autofac;
using Hangfire;
using Hangfire.Logging;
using Hangfire.SqlServer;
using Monster.ConsoleApp.Feeder;
using Monster.Middle;
using Monster.Middle.Config;
using Monster.Middle.Hangfire;
using Push.Foundation.Utilities.Helpers;
using Push.Foundation.Utilities.Logging;


namespace Monster.ConsoleApp
{
    class Program
    {
        private const string RunHangfireBackgroundOption = "1";
        private const string RunShopifyOrderFeederOption = "2";

        static void Main(string[] args)
        {
            Console.WriteLine($"Bridge Console App");
            Console.WriteLine($"++++++++++++++++++");
            Console.WriteLine($"Logic Automated LLC - all rights reserved");
            Console.WriteLine();
            Console.WriteLine($"{RunHangfireBackgroundOption} - RunPaymentsForOrders Hangfire Background Service");
            Console.WriteLine($"{RunShopifyOrderFeederOption} - RunPaymentsForOrders Shopify Test Order Feeder");
            Console.WriteLine(Environment.NewLine + "Make a selection and hit ENTER:");

            var input = Console.ReadLine();

            if (input == RunHangfireBackgroundOption)
            {
                RunHangFireBackgroundService();
            }
            if (input == RunShopifyOrderFeederOption)
            {
                RunShopifyOrderFeeder();
            }

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

            var workerCount = 
                ConfigurationManager
                    .AppSettings["HangFireWorkerCount"]
                    .ToIntegerAlt(10);

            options.WorkerCount = workerCount;

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
            LogProvider.SetCurrentLogProvider(new HangFireLogProvider());

            return container;
        }



        static void RunShopifyOrderFeeder()
        {
            var builder = new ContainerBuilder();
            MiddleAutofac.Build(builder);
            builder.RegisterType<ShopifyDataFeeder>().InstancePerLifetimeScope();
            var container = builder.Build();
            using (var scope = container.BeginLifetimeScope())
            {
                var logger = scope.Resolve<IPushLogger>();

                try
                {
                    var feeder = scope.Resolve<ShopifyDataFeeder>();
                    feeder.Run();
                }
                catch (Exception ex)
                {
                    logger.Error(ex);
                }
            }
        }

    }
}

