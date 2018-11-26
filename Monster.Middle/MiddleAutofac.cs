using Autofac;
using Monster.Acumatica;
using Monster.Acumatica.BankImportApi;
using Monster.Middle.Config;
using Monster.Middle.Persist.Multitenant;
using Monster.Middle.Persist.Multitenant.Acumatica;
using Monster.Middle.Persist.Multitenant.Etc;
using Monster.Middle.Persist.Multitenant.Shopify;
using Monster.Middle.Persist.Multitenant.Sync;
using Monster.Middle.Persist.Sys;
using Monster.Middle.Processes.Inventory;
using Monster.Middle.Processes.Inventory.Services;
using Monster.Middle.Processes.Inventory.Workers;
using Monster.Middle.Processes.Orders;
using Monster.Middle.Processes.Orders.Workers;
using Monster.Middle.Processes.Payouts;
using Monster.Middle.Processes.Payouts.Workers;
using Monster.Middle.Services;
using Push.Foundation.Utilities.Logging;
using Push.Foundation.Web;
using Push.Foundation.Utilities.Security;
using Push.Shopify;


namespace Monster.Middle
{
    public class MiddleAutofac
    {
        public static string LoggerName = "Monster.System";

        public static IContainer Build()
        {
            var builder = new ContainerBuilder();

            // Register Push Foundation stuff
            FoundationWebAutofac.Build(builder);
            
            // Register Acumatica library and inject settings
            AcumaticaHttpAutofac.Build(builder);
            
            // Register Shopify library and inject settings
            ShopifyApiAutofac.Build(builder);
            

            // Crypto faculties
            builder.Register<ICryptoService>(
                x =>
                {
                    var settings = MonsterConfig.Settings;
                    return new AesCrypto(settings.EncryptKey, settings.EncryptIv);
                });
            
            // Logging registrations
            builder.RegisterType<DefaultFormatter>()
                .As<ILogFormatter>()
                .InstancePerLifetimeScope();

            builder.Register(x => new NLogger(LoggerName, x.Resolve<ILogFormatter>()))
                .As<IPushLogger>()
                .InstancePerLifetimeScope();
            
            // TODO *** Need to implement this!!
            //.InstancePerBackgroundJobIfTrue(containerForHangFire);

            // System-level Persistence always uses the MonsterConfig 
            // ... for its Connection String
            builder.Register<SystemRepository>(x =>
            {
                var connectionString
                    = MonsterConfig.Settings.SystemDatabaseConnection;

                return new SystemRepository(connectionString);

            }).SingleInstance();


            // Multitenant Persistence
            builder.RegisterType<PersistContext>().InstancePerLifetimeScope();
            builder.RegisterType<BatchStateRepository>().InstancePerLifetimeScope();
            builder.RegisterType<TenantRepository>().InstancePerLifetimeScope();
            
            builder.RegisterType<ShopifyInventoryRepository>().InstancePerLifetimeScope();
            builder.RegisterType<ShopifyOrderRepository>().InstancePerLifetimeScope();
            builder.RegisterType<ShopifyPayoutRepository>().InstancePerLifetimeScope();

            builder.RegisterType<AcumaticaOrderRepository>().InstancePerLifetimeScope();
            builder.RegisterType<AcumaticaInventoryRepository>().InstancePerLifetimeScope();

            builder.RegisterType<SyncOrderRepository>().InstancePerLifetimeScope();
            builder.RegisterType<SyncInventoryRepository>().InstancePerLifetimeScope();
            

            // Tenant Context
            builder.RegisterType<TenantContext>().InstancePerLifetimeScope();
            
            // Misc
            builder.RegisterType<TimeZoneService>().InstancePerLifetimeScope();


            // Inventory synchronization workers
            builder.RegisterType<AcumaticaInventoryPull>().InstancePerLifetimeScope();
            builder.RegisterType<AcumaticaWarehousePull>().InstancePerLifetimeScope();

            builder.RegisterType<AcumaticaInventorySync>().InstancePerLifetimeScope();
            builder.RegisterType<AcumaticaWarehouseSync>().InstancePerLifetimeScope();

            builder.RegisterType<ShopifyLocationPull>().InstancePerLifetimeScope();
            builder.RegisterType<ShopifyInventoryPull>().InstancePerLifetimeScope();

            builder.RegisterType<ShopifyLocationSync>().InstancePerLifetimeScope();
            builder.RegisterType<ShopifyInventorySync>().InstancePerLifetimeScope();

            builder.RegisterType<InventoryStatusService>().InstancePerLifetimeScope();
            builder.RegisterType<InventoryManager>().InstancePerLifetimeScope();
            

            // Order synchronization workers
            builder.RegisterType<ShopifyCustomerPull>().InstancePerLifetimeScope();
            builder.RegisterType<ShopifyOrderPull>().InstancePerLifetimeScope();
            builder.RegisterType<ShopifyTransactionPull>().InstancePerLifetimeScope();

            builder.RegisterType<ShopifyFulfillmentSync>().InstancePerLifetimeScope();

            builder.RegisterType<AcumaticaCustomerPull>().InstancePerLifetimeScope();
            builder.RegisterType<AcumaticaOrderPull>().InstancePerLifetimeScope();
            builder.RegisterType<AcumaticaShipmentPull>().InstancePerLifetimeScope();

            builder.RegisterType<AcumaticaCustomerSync>().InstancePerLifetimeScope();
            builder.RegisterType<AcumaticaOrderSync>().InstancePerLifetimeScope();
            builder.RegisterType<AcumaticaShipmentSync>().InstancePerLifetimeScope();
            builder.RegisterType<AcumaticaRefundSync>().InstancePerLifetimeScope();
            builder.RegisterType<AcumaticaPaymentSync>().InstancePerLifetimeScope();

            builder.RegisterType<OrderManager>().InstancePerLifetimeScope();


            // Payout Processes
            builder.RegisterType<Screen>().InstancePerLifetimeScope();
            builder.RegisterType<BankImportService>().InstancePerLifetimeScope();
            builder.RegisterType<ShopifyPayoutPullWorker>().InstancePerLifetimeScope();
            builder.RegisterType<PayoutProcess>().InstancePerLifetimeScope();


            return builder.Build();
        }
    }
}

