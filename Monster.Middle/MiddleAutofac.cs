using Autofac;
using Monster.Acumatica;
using Monster.Acumatica.BankImportApi;
using Monster.Middle.Config;
using Monster.Middle.Hangfire;
using Monster.Middle.Persist.Multitenant;
using Monster.Middle.Processes.Acumatica;
using Monster.Middle.Processes.Acumatica.Persist;
using Monster.Middle.Processes.Acumatica.Workers;
using Monster.Middle.Processes.Payouts;
using Monster.Middle.Processes.Payouts.Workers;
using Monster.Middle.Processes.Shopify;
using Monster.Middle.Processes.Shopify.Persist;
using Monster.Middle.Processes.Shopify.Workers;
using Monster.Middle.Processes.Sync.Directors;
using Monster.Middle.Processes.Sync.Inventory;
using Monster.Middle.Processes.Sync.Inventory.Services;
using Monster.Middle.Processes.Sync.Inventory.Workers;
using Monster.Middle.Processes.Sync.Orders;
using Monster.Middle.Processes.Sync.Orders.Workers;
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

        public static ContainerBuilder Build(ContainerBuilder builder)
        {
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
            
            // TODO *** Need to implement this for Batch stuff!!
            //.InstancePerBackgroundJobIfTrue(containerForHangFire);

            // System-level Persistence always uses the MonsterConfig 
            // ... for its Connection String
            builder.Register(x =>
            {
                // TODO - replace with Default Connections string...?
                var connectionString
                    = MonsterConfig.Settings.SystemDatabaseConnection;

                return new Persist.Sys.Repositories.SystemRepository(connectionString);

            }).SingleInstance();

            
            // Multitenant Persistence
            builder.RegisterType<TenantContext>().InstancePerLifetimeScope();
            builder.RegisterType<PersistContext>().InstancePerLifetimeScope();
            builder.RegisterType<TenantRepository>().InstancePerLifetimeScope();

            // Job Running components
            builder.RegisterType<Persist.Sys.Repositories.SystemRepository>().InstancePerLifetimeScope();
            builder.RegisterType<StateRepository>().InstancePerLifetimeScope();
            builder.RegisterType<BackgroundJobRunner>().InstancePerLifetimeScope();
            builder.RegisterType<HangfireService>().InstancePerLifetimeScope();


            // Shopify Pull Process
            builder.RegisterType<ShopifyBatchRepository>().InstancePerLifetimeScope();
            builder.RegisterType<ShopifyInventoryRepository>().InstancePerLifetimeScope();
            builder.RegisterType<ShopifyOrderRepository>().InstancePerLifetimeScope();
            builder.RegisterType<ShopifyPayoutRepository>().InstancePerLifetimeScope();

            builder.RegisterType<ShopifyLocationPull>().InstancePerLifetimeScope();
            builder.RegisterType<ShopifyInventoryPull>().InstancePerLifetimeScope();
            builder.RegisterType<ShopifyCustomerPull>().InstancePerLifetimeScope();
            builder.RegisterType<ShopifyOrderPull>().InstancePerLifetimeScope();
            builder.RegisterType<ShopifyTransactionPull>().InstancePerLifetimeScope();
            builder.RegisterType<ShopifyManager>().InstancePerLifetimeScope();


            // Acumatica Pull Process
            builder.RegisterType<AcumaticaBatchRepository>().InstancePerLifetimeScope();
            builder.RegisterType<AcumaticaOrderRepository>().InstancePerLifetimeScope();
            builder.RegisterType<AcumaticaInventoryRepository>().InstancePerLifetimeScope();

            builder.RegisterType<AcumaticaWarehousePull>().InstancePerLifetimeScope();
            builder.RegisterType<AcumaticaInventoryPull>().InstancePerLifetimeScope();
            builder.RegisterType<AcumaticaCustomerPull>().InstancePerLifetimeScope();
            builder.RegisterType<AcumaticaOrderPull>().InstancePerLifetimeScope();
            builder.RegisterType<AcumaticaShipmentPull>().InstancePerLifetimeScope();
            builder.RegisterType<AcumaticaInvoicePull>().InstancePerLifetimeScope();
            builder.RegisterType<AcumaticaManager>().InstancePerLifetimeScope();
            

            // Sync Inventory Persistence
            builder.RegisterType<SyncInventoryRepository>().InstancePerLifetimeScope();
            builder.RegisterType<InventoryStatusService>().InstancePerLifetimeScope();
            builder.RegisterType<ShopifyLocationSync>().InstancePerLifetimeScope();
            builder.RegisterType<ShopifyInventorySync>().InstancePerLifetimeScope();
            builder.RegisterType<AcumaticaInventorySync>().InstancePerLifetimeScope();
            builder.RegisterType<AcumaticaWarehouseSync>().InstancePerLifetimeScope();
            builder.RegisterType<InventoryManager>().InstancePerLifetimeScope();

            // Order Synchronization components
            builder.RegisterType<SyncOrderRepository>().InstancePerLifetimeScope();            
            builder.RegisterType<ShopifyFulfillmentSync>().InstancePerLifetimeScope();
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

            // Director Components
            builder.RegisterType<SyncDirector>().InstancePerLifetimeScope();

            // Misc
            builder.RegisterType<TimeZoneService>().InstancePerLifetimeScope();

            return builder;
        }
    }
}

