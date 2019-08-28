using System.Data.Common;
using System.Data.SqlClient;
using Autofac;
using Monster.Acumatica;
using Monster.Acumatica.BankImportApi;
using Monster.Middle.Config;
using Monster.Middle.Hangfire;
using Monster.Middle.Identity;
using Monster.Middle.Persist.Instance;
using Monster.Middle.Persist.Master;
using Monster.Middle.Processes.Acumatica.Persist;
using Monster.Middle.Processes.Acumatica.Services;
using Monster.Middle.Processes.Acumatica.Workers;
using Monster.Middle.Processes.Payouts;
using Monster.Middle.Processes.Payouts.Workers;
using Monster.Middle.Processes.Shopify.Persist;
using Monster.Middle.Processes.Shopify.Workers;
using Monster.Middle.Processes.Sync.Managers;
using Monster.Middle.Processes.Sync.Persist;
using Monster.Middle.Processes.Sync.Services;
using Monster.Middle.Processes.Sync.Status;
using Monster.Middle.Processes.Sync.Workers.Inventory;
using Monster.Middle.Processes.Sync.Workers.Orders;
using Monster.Middle.Services;
using Push.Foundation.Utilities.Logging;
using Push.Foundation.Utilities.Security;
using Push.Foundation.Web;
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

            // Use this connection string for IdentityDbContext OWIN stuff
            builder
                .Register(ctx =>
                {
                    var connection = new SqlConnection(connectionString);
                    connection.Open();
                    return connection;
                })
                .As<SqlConnection>()
                .As<DbConnection>()
                .InstancePerLifetimeScope();

            // Persistence - Master-level
            builder.RegisterType<SystemRepository>().InstancePerLifetimeScope();

            // Persistence - Instance-level
            builder.RegisterType<PersistContext>().InstancePerLifetimeScope();
            builder.RegisterType<ConnectionRepository>().InstancePerLifetimeScope();
            builder.RegisterType<ConnectionContext>().InstancePerLifetimeScope();

            // Job Running components
            builder.RegisterType<JobRepository>().InstancePerLifetimeScope();
            builder.RegisterType<OneTimeJobService>().InstancePerLifetimeScope();
            builder.RegisterType<RecurringJobService>().InstancePerLifetimeScope();
            builder.RegisterType<JobRunner>().InstancePerLifetimeScope();
            builder.RegisterType<JobStatusService>().InstancePerLifetimeScope();

            // Process Registrations
            RegisterIdentityPlumbing(builder);
            RegisterShopifyProcess(builder);            
            RegisterAcumaticaProcess(builder);            
            RegisterSyncProcess(builder);
            RegisterPayoutProcess(builder);

            // Misc
            builder.RegisterType<TimeZoneTranslator>().InstancePerLifetimeScope();

            // Crypto faculties
            builder.Register<ICryptoService>(x => new AesCrypto(
                MonsterConfig.Settings.EncryptKey, MonsterConfig.Settings.EncryptIv));

            return builder;
        }

        private static void RegisterIdentityPlumbing(ContainerBuilder builder)
        {
            builder.RegisterType<IdentityService>();
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

            builder.RegisterType<ReferenceDataService>().InstancePerLifetimeScope();
        }

        private static void RegisterSyncProcess(ContainerBuilder builder)
        {
            // Inventory 
            builder.RegisterType<SyncInventoryRepository>().InstancePerLifetimeScope();
            builder.RegisterType<ShopifyInventorySync>().InstancePerLifetimeScope();
            builder.RegisterType<AcumaticaInventorySync>().InstancePerLifetimeScope();
            builder.RegisterType<WarehouseLocationSync>().InstancePerLifetimeScope();
            
            // Orders
            builder.RegisterType<SyncOrderRepository>().InstancePerLifetimeScope();
            builder.RegisterType<ShopifyFulfillmentSync>().InstancePerLifetimeScope();
            builder.RegisterType<AcumaticaCustomerSync>().InstancePerLifetimeScope();
            builder.RegisterType<AcumaticaOrderSync>().InstancePerLifetimeScope();
            builder.RegisterType<AcumaticaShipmentSync>().InstancePerLifetimeScope();
            builder.RegisterType<AcumaticaRefundSync>().InstancePerLifetimeScope();
            builder.RegisterType<AcumaticaOrderPaymentSync>().InstancePerLifetimeScope();
            
            // Services
            builder.RegisterType<UrlService>().InstancePerLifetimeScope();            
            builder.RegisterType<StateRepository>().InstancePerLifetimeScope();
            builder.RegisterType<PreferencesRepository>().InstancePerLifetimeScope();
            builder.RegisterType<ExecutionLogService>().InstancePerLifetimeScope();
            builder.RegisterType<AcumaticaTimeZoneService>().InstancePerLifetimeScope();

            // Status
            builder.RegisterType<ConfigStatusService>().InstancePerLifetimeScope();
            builder.RegisterType<OrderStatusService>().InstancePerLifetimeScope();
            builder.RegisterType<ShipmentStatusService>().InstancePerLifetimeScope();
            builder.RegisterType<FulfillmentStatusService>().InstancePerLifetimeScope();
            
            // Management Objects
            builder.RegisterType<SyncManager>().InstancePerLifetimeScope();
            builder.RegisterType<SyncDirector>().InstancePerLifetimeScope();
        }
    }
}

