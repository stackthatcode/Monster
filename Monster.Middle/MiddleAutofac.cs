using System.Data.SqlClient;
using Autofac;
using Monster.Acumatica;
using Monster.Acumatica.BankImportApi;
using Monster.Middle.Config;
using Monster.Middle.Hangfire;
using Monster.Middle.Persist.Multitenant;
using Monster.Middle.Persist.Sys;
using Monster.Middle.Processes.Acumatica;
using Monster.Middle.Processes.Acumatica.Persist;
using Monster.Middle.Processes.Acumatica.Workers;
using Monster.Middle.Processes.Payouts;
using Monster.Middle.Processes.Payouts.Workers;
using Monster.Middle.Processes.Shopify;
using Monster.Middle.Processes.Shopify.Persist;
using Monster.Middle.Processes.Shopify.Workers;
using Monster.Middle.Processes.Sync;
using Monster.Middle.Processes.Sync.Inventory;
using Monster.Middle.Processes.Sync.Inventory.Workers;
using Monster.Middle.Processes.Sync.Orders;
using Monster.Middle.Processes.Sync.Orders.Workers;
using Monster.Middle.Processes.Sync.Status;
using Monster.Middle.Security;
using Monster.Middle.Services;
using Push.Foundation.Utilities.Logging;
using Push.Foundation.Web;
using Push.Foundation.Utilities.Security;
using Push.Foundation.Web.Identity;
using Push.Shopify;


namespace Monster.Middle
{
    public class MiddleAutofac
    {
        public static string LoggerName = "Monster.System";

        public static ContainerBuilder Build(ContainerBuilder builder)
        {
            // Register external dependencies
            FoundationWebAutofac.Build(builder);            
            AcumaticaHttpAutofac.Build(builder);            
            ShopifyApiAutofac.Build(builder);
            
            // Logging registrations
            builder.RegisterType<DefaultFormatter>()
                .As<ILogFormatter>()
                .InstancePerLifetimeScope();

            builder.Register(x => new NLogger(LoggerName, x.Resolve<ILogFormatter>()))
                .As<IPushLogger>()
                .InstancePerLifetimeScope();

            // TODO *** decide if it's necessary to implement this for Batch stuff!!
            //.InstancePerBackgroundJobIfTrue(containerForHangFire);


            // System-level Persistence always uses the MonsterConfig for its Connection String
            var connectionString = MonsterConfig.Settings.SystemDatabaseConnection;
            builder
                .Register(x => new SystemRepository(connectionString))
                .InstancePerLifetimeScope();

            // ... and use same for IdentityDbContext OWIN stuff
            builder.Register(
                    ctx => new IdentityDbContext(new SqlConnection(connectionString)))
                .InstancePerLifetimeScope();
            
            // Crypto faculties
            builder.Register<ICryptoService>(x => new AesCrypto(
                    MonsterConfig.Settings.EncryptKey, MonsterConfig.Settings.EncryptIv));

            // Multitenant Persistence
            builder.RegisterType<ConnectionRepository>().InstancePerLifetimeScope();
            builder.RegisterType<PersistContext>().InstancePerLifetimeScope();
            builder.RegisterType<ConnectionContext>().InstancePerLifetimeScope();
            builder.RegisterType<PreferencesRepository>().InstancePerLifetimeScope();
            builder.RegisterType<StateRepository>().InstancePerLifetimeScope();
            builder.RegisterType<ExecutionLogRepository>().InstancePerLifetimeScope();

            // Job Running components
            builder.RegisterType<JobRunner>().InstancePerLifetimeScope();
            builder.RegisterType<ExclusiveJobRunner>().InstancePerLifetimeScope();
            builder.RegisterType<OneTimeJobService>().InstancePerLifetimeScope();
            builder.RegisterType<RecurringJobService>().InstancePerLifetimeScope();

            // Process Registrations
            RegisterShopifyProcess(builder);            
            RegisterAcumaticaProcess(builder);            
            RegisterSyncProcess(builder);
            RegisterPayoutProcess(builder);

            // Misc
            builder.RegisterType<InstanceTimeZoneService>().InstancePerLifetimeScope();
            builder.RegisterType<TimeZoneTranslator>().InstancePerLifetimeScope();

            return builder;
        }

        private static void RegisterPayoutProcess(ContainerBuilder builder)
        {
            builder.RegisterType<Screen>().InstancePerLifetimeScope();
            builder.RegisterType<BankImportService>().InstancePerLifetimeScope();
            builder.RegisterType<ShopifyPayoutPullWorker>().InstancePerLifetimeScope();
            builder.RegisterType<PayoutProcess>().InstancePerLifetimeScope();
        }

        private static void RegisterShopifyProcess(ContainerBuilder builder)
        {
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
        }

        private static void RegisterAcumaticaProcess(ContainerBuilder builder)
        {
            // Acumatica Pull Process
            builder.RegisterType<AcumaticaBatchRepository>().InstancePerLifetimeScope();
            builder.RegisterType<AcumaticaOrderRepository>().InstancePerLifetimeScope();
            builder.RegisterType<AcumaticaInventoryRepository>().InstancePerLifetimeScope();

            builder.RegisterType<AcumaticaWarehousePull>().InstancePerLifetimeScope();
            builder.RegisterType<AcumaticaInventoryPull>().InstancePerLifetimeScope();
            builder.RegisterType<AcumaticaCustomerPull>().InstancePerLifetimeScope();
            builder.RegisterType<AcumaticaOrderPull>().InstancePerLifetimeScope();
            builder.RegisterType<AcumaticaShipmentPull>().InstancePerLifetimeScope();
            builder.RegisterType<AcumaticaReferencePull>().InstancePerLifetimeScope();

            builder.RegisterType<AcumaticaManager>().InstancePerLifetimeScope();
        }

        private static void RegisterSyncProcess(ContainerBuilder builder)
        {
            // Inventory 
            builder.RegisterType<SyncInventoryRepository>().InstancePerLifetimeScope();
            builder.RegisterType<ShopifyInventorySync>().InstancePerLifetimeScope();
            builder.RegisterType<AcumaticaInventorySync>().InstancePerLifetimeScope();
            builder.RegisterType<WarehouseLocationSync>().InstancePerLifetimeScope();
            builder.RegisterType<InventorySyncManager>().InstancePerLifetimeScope();

            // Orders
            builder.RegisterType<SyncOrderRepository>().InstancePerLifetimeScope();
            builder.RegisterType<ShopifyFulfillmentSync>().InstancePerLifetimeScope();
            builder.RegisterType<AcumaticaCustomerSync>().InstancePerLifetimeScope();
            builder.RegisterType<AcumaticaOrderSync>().InstancePerLifetimeScope();
            builder.RegisterType<AcumaticaShipmentSync>().InstancePerLifetimeScope();
            builder.RegisterType<AcumaticaRefundSync>().InstancePerLifetimeScope();
            builder.RegisterType<AcumaticaOrderPaymentSync>().InstancePerLifetimeScope();
            builder.RegisterType<OrderManager>().InstancePerLifetimeScope();

            // Status
            builder.RegisterType<StatusService>().InstancePerLifetimeScope();
            builder.RegisterType<ReferenceDataService>().InstancePerLifetimeScope();
            builder.RegisterType<UrlService>().InstancePerLifetimeScope();

            // Director Components
            builder.RegisterType<SyncDirector>().InstancePerLifetimeScope();
        }
    }
}

