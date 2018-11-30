using System;
using System.Configuration;
using Autofac;
using Hangfire;
using Hangfire.Logging;
using Hangfire.SqlServer;
using Monster.ConsoleApp.Monster;
using Monster.Middle;
using Monster.Middle.HangFire;
using Push.Foundation.Utilities.Helpers;


namespace Monster.ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            RunHangFireBackgroundService();
        }


        static void RunHangFireBackgroundService()
        {            
            ConfigureHangFire();

            var options = new BackgroundJobServerOptions()
                {
                    SchedulePollingInterval = new TimeSpan(0, 0, 0, 1),
                    //Queues = QueueChannel.All, ** Don't think we're using this...
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
            var container = MiddleAutofac.Build(builder).Build();

            // Set the HangFire storage
            JobStorage.Current = new SqlServerStorage("DefaultConnection");
            
            // Wire in the Autofac container
            GlobalConfiguration.Configuration.UseAutofacActivator(container);

            // HangFireLogProvider -> HangFireLogger -> LoggerSingleton
            LogProvider.SetCurrentLogProvider(new HangFireLogProvider());

            return container;
        }

        

        static void RunTestingHarness()
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

