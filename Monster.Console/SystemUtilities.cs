using System;
using System.Configuration;
using Autofac;
using Hangfire;
using Hangfire.Logging;
using Hangfire.SqlServer;
using Monster.Acumatica.Api;
using Monster.Acumatica.Api.SalesOrder;
using Monster.Acumatica.Http;
using Monster.Middle;
using Monster.Middle.Config;
using Monster.Middle.Identity;
using Monster.Middle.Misc.Hangfire;
using Monster.Middle.Persist.Instance;
using Monster.Middle.Persist.Master;
using Monster.Middle.Processes.Acumatica.Persist;
using Monster.Middle.Processes.Shopify.Persist;
using Monster.Middle.Processes.Shopify.Workers;
using Monster.Middle.Processes.Sync.Model.TaxTransfer;
using Monster.Middle.Processes.Sync.Workers;
using Push.Foundation.Utilities.General;
using Push.Foundation.Utilities.Helpers;
using Push.Foundation.Utilities.Json;
using Push.Foundation.Utilities.Logging;


namespace Monster.ConsoleApp
{
    public class SystemUtilities
    {
        private static string TestInstanceId = "51AA413D-E679-4F38-BA47-68129B3F9212";
        private static string DefaultSalesOrderNbr = "000046";


        public static Guid SolicitInstanceId()
        {
            Console.WriteLine(Environment.NewLine + $"Enter Instance Id (Default Id: {TestInstanceId})");
            return Guid.Parse(Console.ReadLine().IsNullOrEmptyAlt(TestInstanceId));
        }

        public static long SolicitShopifyId()
        {
            Console.WriteLine(Environment.NewLine + "Enter Shopify Order Id (Default Id: 1840328409132)");
            return Console.ReadLine().IsNullOrEmptyAlt("1840328409132").ToLong();
        }

        public static string SolicitAcumaticaSalesOrderId()
        {
            Console.WriteLine(Environment.NewLine
                              + $"Enter Acumatica Sales Order ID (Default ID: {DefaultSalesOrderNbr})");
            var orderNbr = Console.ReadLine().Trim().IsNullOrEmptyAlt(DefaultSalesOrderNbr);
            return orderNbr;
        }

        public static void RunHangFireBackgroundService()
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

        public static IContainer ConfigureHangFire()
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

        public static void RunViewShopifyOrderAndTaxTransfer()
        {
            var instanceId = SolicitInstanceId();
            var shopifyOrderId = SolicitShopifyId();
            
            AutofacRunner.RunInLifetimeScope(scope =>
            {
                var logger = scope.Resolve<IPushLogger>();
                var instanceContext = scope.Resolve<InstanceContext>();
                var shopifyOrderGet = scope.Resolve<ShopifyOrderGet>();
                var repository = scope.Resolve<ShopifyOrderRepository>();

                instanceContext.Initialize(instanceId);

                shopifyOrderGet.Run(shopifyOrderId);
                var shopifyOrder = repository.RetrieveOrder(shopifyOrderId);
                logger.Info("Shopify Order JSON" + Environment.NewLine + shopifyOrder.ShopifyJson);

                var taxTransfer = shopifyOrder.ToTaxTransfer();
                logger.Info("Shopify Tax Transfer" + Environment.NewLine + taxTransfer.SerializeToJson());
            });
        }

        public static void RunViewAcumaticaTaxTransfer()
        {
            var instanceId = SolicitInstanceId();
            var salesOrderNbr = SolicitAcumaticaSalesOrderId();

            AutofacRunner.RunInLifetimeScope(scope =>
            {
                var logger = scope.Resolve<IPushLogger>();
                var instanceContext = scope.Resolve<InstanceContext>();
                var acumaticaContext = scope.Resolve<AcumaticaHttpContext>();
                var salesOrderClient = scope.Resolve<SalesOrderClient>();

                instanceContext.Initialize(instanceId);

                acumaticaContext.SessionRun(() =>
                {
                    var json = salesOrderClient
                        .RetrieveSalesOrder(salesOrderNbr, SalesOrderType.SO, Expand.Details_Totals);

                    var salesOrderObj = json.ToSalesOrderObj();
                    var taxSnapshot = salesOrderObj.custom.Document.UsrTaxSnapshot;
                    var taxJson = taxSnapshot.value.Unzip();

                    logger.Info("Acumatica Tax Transfer");
                    logger.Info(taxJson);
                });
            });
        }

        public static void RunShopifyOrderGetToAcumaticaOrderPut()
        {
            var instanceId = SolicitInstanceId();
            var shopifyOrderId = SolicitShopifyId();
            AutofacRunner.RunInLifetimeScope(scope =>
            {
                var instanceContext = scope.Resolve<InstanceContext>();
                instanceContext.Initialize(instanceId);

                Console.WriteLine($"Processing Shopify Order Id: {shopifyOrderId}");

                var shopifyOrderGet = scope.Resolve<ShopifyOrderGet>();
                var order = shopifyOrderGet.Run(shopifyOrderId).order;

                var orderSync = scope.Resolve<AcumaticaOrderPut>();

                var acumaticaContext = scope.Resolve<AcumaticaHttpContext>();
                acumaticaContext.SessionRun(() => orderSync.RunOrder(shopifyOrderId));
            });
        }


        // User management functions
        //
        public static void ProvisionNewUserAccount()
        {
            Console.WriteLine(
                Environment.NewLine + "Enter New User's Email Address (which will be used as User ID)");
            var email = Console.ReadLine();

            Console.WriteLine(Environment.NewLine + "Enter New User's Shopify Domain");
            var domain = Console.ReadLine();

            var msg = $"Create a new Account for User ID: {email} - Shopify Domain: {domain}";
            if (!CommandLineFuncs.Confirm(msg))
            {
                return;
            }

            Action<ILifetimeScope> process = scope =>
            {
                var service = scope.Resolve<ProvisioningService>();
                var user = service.ProvisionNewAccount(email, domain).Result;
                Console.WriteLine(Environment.NewLine + "Created User...");
            };

            AutofacRunner.RunInLifetimeScope(process);
        }

        public static void HydrateSecurityConfig()
        {
            AutofacRunner.RunInLifetimeScope(scope =>
            {
                var service = scope.Resolve<ProvisioningService>();
                service.PopulateRolesAndAdmin();
            });
        }

        public static void AssignInstance()
        {
            Console.WriteLine(Environment.NewLine + "Enter Account Email Address:");
            var email = Console.ReadLine();

            Console.WriteLine(Environment.NewLine + "Enter Domain Name of Shopify store:");
            var domain = Console.ReadLine();

            Console.WriteLine(Environment.NewLine + "Enter a Nick Name for the Instance assignment:");
            var nickname = Console.ReadLine();

            AutofacRunner.RunInLifetimeScope(scope =>
            {
                var service = scope.Resolve<ProvisioningService>();
                service.AssignNextAvailableInstance(email, domain, nickname);
            });
        }

        public static void RevokeInstance()
        {
            Console.WriteLine(Environment.NewLine + "Enter Domain for Instance");
            var domain = Console.ReadLine();

            AutofacRunner.RunInLifetimeScope(scope =>
            {
                var service = scope.Resolve<ProvisioningService>();
                service.RevokeInstanceByDomain(domain);
            });
        }

        public static void DisableInstance()
        {
            var instanceId = SolicitInstanceId();
            AutofacRunner.RunInLifetimeScope(scope =>
            {
                var repository = scope.Resolve<MasterRepository>();
                repository.UpdateInstanceEnabled(instanceId, false);
            });
        }

        public static void EnableInstance()
        {
            var instanceId = SolicitInstanceId();
            AutofacRunner.RunInLifetimeScope(scope =>
            {
                var repository = scope.Resolve<MasterRepository>();
                repository.UpdateInstanceEnabled(instanceId, true);
            });
        }

        public static void ListInstances()
        {
            AutofacRunner.RunInLifetimeScope(scope =>
            {
                var repository = scope.Resolve<MasterRepository>();
                var instances = repository.RetrieveInstances();
                foreach (var instance in instances)
                {
                    var output = 
                        $"{instance.Id} - {instance.ConnectionString ?? "(connection string missing)"}" + Environment.NewLine +
                        $"{instance.OwnerDomain ?? "(unassigned)"} - {instance.OwnerNickname ?? "(unassigned)"}" +
                        Environment.NewLine;
                    Console.WriteLine(output);
                }
            });
        }
    }
}
