using System;
using System.Configuration;
using Autofac;
using Hangfire;
using Hangfire.Logging;
using Hangfire.SqlServer;
using Monster.Middle;
using Monster.Middle.Config;
using Monster.Middle.Hangfire;
using Push.Foundation.Utilities.Helpers;


namespace Monster.ConsoleApp
{
    class Program
    {
        private const string RunHangfireBackground = "1";
        private const string RunShopifyOrderFeeder = "2";

        static void Main(string[] args)
        {
            Console.WriteLine($"Bridge Console App");
            Console.WriteLine($"++++++++++++++++++");
            Console.WriteLine($"Logic Automated LLC - all rights reserved");
            Console.WriteLine(Environment.NewLine);
            Console.WriteLine($"{RunHangfireBackground} - Run Hangfire Background Service");
            Console.WriteLine($"{RunShopifyOrderFeeder} - Run Shopify Order Feeder");
            Console.WriteLine(Environment.NewLine + "Make a selection and hit ENTER:");

            var input = Console.ReadLine();

            RunHangFireBackgroundService();

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

        public static IContainer ConfigureHangFire()
        {
            var builder = new ContainerBuilder();
            MiddleAutofac.Build(builder);
            
            // *** Inject
            var container = builder.Build();
            
            // Set the HangFire storage
            var systemDbConnection = MonsterConfig.Settings.SystemDatabaseConnection;
            
            JobStorage.Current = new SqlServerStorage(systemDbConnection);
            
            // Wire in the Autofac container
            GlobalConfiguration.Configuration.UseAutofacActivator(container);

            // HangFireLogProvider -> HangFireLogger -> LoggerSingleton
            LogProvider.SetCurrentLogProvider(new HangFireLogProvider());

            return container;
        }

    }
}

