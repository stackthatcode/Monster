﻿using System;
using System.Configuration;
using Autofac;
using Hangfire;
using Hangfire.Logging;
using Hangfire.SqlServer;
using Monster.ConsoleApp.Feeder;
using Monster.Middle;
using Monster.Middle.Config;
using Monster.Middle.Hangfire;
using Monster.Middle.Identity;
using Push.Foundation.Utilities.Helpers;
using Push.Foundation.Utilities.Logging;


namespace Monster.ConsoleApp
{
    class Program
    {
        private const string RunHangfireBackgroundOption = "1";
        private const string RunShopifyOrderFeederOption = "2";
        private const string ProvisionNewUserAccountOption = "3";
        private const string HydrateSecurityConfigOption = "4";

        static void Main(string[] args)
        {
            Console.WriteLine($"Bridge Console App");
            Console.WriteLine($"++++++++++++++++++");
            Console.WriteLine($"Logic Automated LLC - all rights reserved");
            Console.WriteLine();
            Console.WriteLine($"{RunHangfireBackgroundOption} - Run Hangfire Background Service");
            Console.WriteLine($"{RunShopifyOrderFeederOption} - Run Shopify Test Order Feeder");
            Console.WriteLine($"{ProvisionNewUserAccountOption} - Provision New User Account");
            Console.WriteLine($"{HydrateSecurityConfigOption} - Hydrate Security Config");
            Console.WriteLine(Environment.NewLine + "Make a selection and hit ENTER:");

            var input = Console.ReadLine();
            if (input == RunHangfireBackgroundOption)
                RunHangFireBackgroundService();
            if (input == RunShopifyOrderFeederOption)
                RunShopifyOrderFeeder();
            if (input == ProvisionNewUserAccountOption)
                ProvisionNewUserAccount();
            if (input == HydrateSecurityConfigOption)
                HydrateSecurityConfig();

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

        static void ProvisionNewUserAccount()
        {
            Console.WriteLine(Environment.NewLine + 
                              "Enter New User's Email Address (which will be used as User ID)");
            var email = Console.ReadLine();

            Console.WriteLine(Environment.NewLine + 
                              "Enter New User's Shopify Domain");
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

            RunInLifetimeScope(process);
        }

        static void HydrateSecurityConfig()
        {
            RunInLifetimeScope(scope =>
            {
                var identityService = scope.Resolve<IdentityService>();
                identityService.HydrateRolesAndAdmin();
            });
        }

        static void RunShopifyOrderFeeder()
        {
            RunInLifetimeScope(
                scope =>
                {
                    var feeder = scope.Resolve<ShopifyDataFeeder>();
                    feeder.Run();
                },
                builder =>
                {
                    builder.RegisterType<ShopifyDataFeeder>().InstancePerLifetimeScope();
                });
        }

        static void RunInLifetimeScope(
                Action<ILifetimeScope> action,  Action<ContainerBuilder> builderPreExec = null)
        {
            var builder = new ContainerBuilder();
            MiddleAutofac.Build(builder);
            if (builderPreExec != null)
            {
                builderPreExec(builder);
            }
            var container = builder.Build();
            using (var scope = container.BeginLifetimeScope())
            {
                var logger = scope.Resolve<IPushLogger>();

                try
                {
                    action(scope);
                }
                catch (Exception ex)
                {
                    logger.Error(ex);
                }
            }
        }
    }
}

