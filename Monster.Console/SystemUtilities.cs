using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Autofac;
using Hangfire;
using Hangfire.Logging;
using Monster.Acumatica.Api;
using Monster.Acumatica.Api.SalesOrder;
using Monster.Acumatica.Http;
using Monster.Middle.Identity;
using Monster.Middle.Misc.Hangfire;
using Monster.Middle.Persist.Instance;
using Monster.Middle.Persist.Master;
using Monster.Middle.Processes.Acumatica.Persist;
using Monster.Middle.Processes.Shopify.Persist;
using Monster.Middle.Processes.Shopify.Workers;
using Monster.Middle.Processes.Sync.Model.FinAnalyzer;
using Monster.Middle.Processes.Sync.Model.TaxTranfser;
using Monster.Middle.Processes.Sync.Workers;
using Monster.TaxTransfer.v2;
using Newtonsoft.Json;
using Push.Foundation.Utilities.General;
using Push.Foundation.Utilities.Helpers;
using Push.Foundation.Utilities.Json;
using Push.Foundation.Utilities.Logging;
using Push.Foundation.Web.Identity;


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
            Console.WriteLine(Environment.NewLine + "Enter Shopify Order Id (Default Id: 1949921378348)");
            return NumberHelpers.ToLong(Console.ReadLine().IsNullOrEmptyAlt("1949921378348	"));
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
            var container = ConsoleAppAutofac.BuildContainer();

            // Configure Hangfire for Background Job
            //
            HangFireConfig.ConfigureStorage();

            GlobalConfiguration.Configuration.UseAutofacActivator(container);
            LogProvider.SetCurrentLogProvider(new HangfireLogProvider());

            var options = new BackgroundJobServerOptions()
            {
                SchedulePollingInterval = new TimeSpan(0, 0, 0, 1),
                WorkerCount = ConfigurationManager.AppSettings["HangFireWorkerCount"].ToIntegerAlt(10),
            };

            using (var server = new BackgroundJobServer(options))
            {
                Console.WriteLine("Hangfire Server started. Press any key to exit...");
                Console.ReadKey();
            }
        }

        

        public static void RunViewShopifyOrderAndTaxTransfer()
        {
            var instanceId = SolicitInstanceId();
            var shopifyOrderId = SolicitShopifyId();
            
            AutofacRunner.RunInScope(scope =>
            {
                var logger = scope.Resolve<IPushLogger>();
                var instanceContext = scope.Resolve<InstanceContext>();
                var shopifyOrderGet = scope.Resolve<ShopifyOrderGet>();
                var repository = scope.Resolve<ShopifyOrderRepository>();

                instanceContext.Initialize(instanceId);

                shopifyOrderGet.Run(shopifyOrderId);
                var shopifyOrder = repository.RetrieveOrder(shopifyOrderId);
                logger.Info("Shopify Order JSON" + Environment.NewLine + 
                            shopifyOrder.ShopifyJson + Environment.NewLine);

                var taxTransfer = shopifyOrder.ToTaxTransfer();
                logger.Info("Shopify Tax Transfer: " + Environment.NewLine + 
                            taxTransfer.SerializeToJson() + Environment.NewLine);

                var serializedTaxTransfer = taxTransfer.Serialize();
                logger.Info($"Shopify Tax Transfer Serialized:" + Environment.NewLine + 
                            serializedTaxTransfer + Environment.NewLine);

                logger.Info($"Shopify Tax Transfer gzipped size: " + 
                            $"{serializedTaxTransfer.ToBase64Zip().Length} bytes" + 
                            Environment.NewLine);

                var deserializedTaxTransfer = serializedTaxTransfer.DeserializeTaxSnapshot();
                logger.Info($"Shopify Tax Transfer Deserialized:" + Environment.NewLine +
                            deserializedTaxTransfer.SerializeToJson() + Environment.NewLine);

                var shopifyOrderObj = shopifyOrder.ToShopifyObj();
                var lineItem = shopifyOrderObj.line_items[0];

                var testCalc = deserializedTaxTransfer
                    .CalculateTax(lineItem.sku, lineItem.UnitPriceAfterDiscount, 1);

                logger.Info("Test Tax Calculation: " + Environment.NewLine +
                            testCalc.SerializeToJson() + Environment.NewLine);
            });
        }

        public static void RunViewAcumaticaTaxTransfer()
        {
            var instanceId = SolicitInstanceId();
            var salesOrderNbr = SolicitAcumaticaSalesOrderId();

            AutofacRunner.RunInScope(scope =>
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
                    var taxJson = Compression.Unzip(taxSnapshot.value);

                    logger.Info("Acumatica Tax Transfer");
                    logger.Info(taxJson);
                });
            });
        }

        public static void RunShopifyOrderGetToAcumaticaOrderPut()
        {
            var instanceId = SolicitInstanceId();
            var shopifyOrderId = SolicitShopifyId();
            AutofacRunner.RunInScope(scope =>
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

            AutofacRunner.RunInScope(process);
        }

        public static void HydrateSecurityConfig()
        {
            AutofacRunner.RunInScope(scope =>
            {
                var service = scope.Resolve<ProvisioningService>();
                service.PopulateRolesAndAdmin();
            });
        }

        public static void ListAllUserAccounts()
        {
            Action<ILifetimeScope> process = scope =>
            {
                var service = scope.Resolve<ProvisioningService>();
                var repository = scope.Resolve<MasterRepository>();
                var roleManager = scope.Resolve<IdentityRoleManager>();
                var users = service.RetrieveUsers();

                foreach (var user in users)
                {
                    var roleNames = new List<string>();
                    foreach (var userRole in user.Roles)
                    {
                        roleNames.Add(roleManager.Roles.First(x => x.Id == userRole.RoleId).Name);
                    }
                    Console.WriteLine($"User: {user.Email} (Id: {user.Id}) - Roles: {roleNames.ToCommaDelimited()}");

                    if (user.Logins.Count > 0)
                    {
                        foreach (var login in user.Logins)
                        {
                            Console.WriteLine($"Login: {login.LoginProvider} - {login.ProviderKey}");
                        }
                    }
                    else
                    {
                        Console.WriteLine("*** User has no ASP.NET Logins ***");
                    }

                    var instances = repository.RetrievesInstanceByUserId(user.Id);
                    if (instances.Count > 0)
                    {
                        foreach (var instance in instances)
                        {
                            Console.WriteLine(
                                $"Instance: {instance.InstanceDatabase} - {instance.OwnerDomain} - {instance.OwnerNickname}");
                        }
                    }
                    else
                    {
                        Console.WriteLine("*** User has no Instance assignments ***");
                    }

                    Console.WriteLine();
                }
            };

            AutofacRunner.RunInScope(process);
        }


        public static void RegisterInstance()
        {
            Console.WriteLine(Environment.NewLine + "Enter Instance database name:");
            var database = Console.ReadLine();

            if (!CommandLineFuncs.Confirm(
                    $"Are you sure you want to register database: {database} as an Instance?"))
            {
                return;
            }

            AutofacRunner.RunInScope(scope =>
            {
                var repository = scope.Resolve<MasterRepository>();
                repository.InsertInstance(database, true);
            });

            Console.WriteLine($"Database {database} registered as an Instance..." + Environment.NewLine );
        }

        public static void AssignInstance()
        {
            Console.WriteLine(Environment.NewLine + "Enter Account Email Address:");
            var email = Console.ReadLine();

            Console.WriteLine(Environment.NewLine + "Enter Domain Name of Shopify store:");
            var domain = Console.ReadLine();

            Console.WriteLine(Environment.NewLine + "Enter a Nick Name for the Instance assignment:");
            var nickname = Console.ReadLine();

            AutofacRunner.RunInScope(scope =>
            {
                var service = scope.Resolve<ProvisioningService>();
                service.AssignNextAvailableInstance(email, domain, nickname);
            });
        }

        public static void RevokeInstance()
        {
            Console.WriteLine(Environment.NewLine + "Enter Domain for Instance");
            var domain = Console.ReadLine();

            AutofacRunner.RunInScope(scope =>
            {
                var service = scope.Resolve<ProvisioningService>();
                service.RevokeInstanceByDomain(domain);
            });
        }

        public static void DisableInstance()
        {
            var instanceId = SolicitInstanceId();
            AutofacRunner.RunInScope(scope =>
            {
                var repository = scope.Resolve<MasterRepository>();
                repository.UpdateInstanceEnabled(instanceId, false);
            });
        }

        public static void EnableInstance()
        {
            var instanceId = SolicitInstanceId();
            AutofacRunner.RunInScope(scope =>
            {
                var repository = scope.Resolve<MasterRepository>();
                repository.UpdateInstanceEnabled(instanceId, true);
            });
        }

        public static void ListInstances()
        {
            AutofacRunner.RunInScope(scope =>
            {
                var repository = scope.Resolve<MasterRepository>();
                var instances = repository.RetrieveInstances();
                foreach (var instance in instances)
                {
                    var output =
                        $"{instance.Id} - {instance.InstanceDatabase ?? "(connection string missing)"}" +
                        Environment.NewLine +
                        $"{instance.OwnerDomain ?? "(unassigned)"} - {instance.OwnerNickname ?? "(unassigned)"}" +
                        $" - Enabled: {instance.IsEnabled}" + Environment.NewLine;
                    Console.WriteLine(output);
                }
            });
        }
    }
}
