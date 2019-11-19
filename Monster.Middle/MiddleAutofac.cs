using System.Data.Common;
using System.Data.SqlClient;
using Autofac;
using Monster.Acumatica;
using Monster.Acumatica.BankImportApi;
using Monster.Middle.Config;
using Monster.Middle.Identity;
using Monster.Middle.Misc.Acumatica;
using Monster.Middle.Misc.External;
using Monster.Middle.Misc.Hangfire;
using Monster.Middle.Misc.Logging;
using Monster.Middle.Misc.Shopify;
using Monster.Middle.Misc.State;
using Monster.Middle.Persist.Instance;
using Monster.Middle.Persist.Master;
using Monster.Middle.Processes.Acumatica;
using Monster.Middle.Processes.Acumatica.Persist;
using Monster.Middle.Processes.Acumatica.Services;
using Monster.Middle.Processes.Acumatica.Workers;
using Monster.Middle.Processes.Payouts;
using Monster.Middle.Processes.Payouts.Workers;
using Monster.Middle.Processes.Shopify;
using Monster.Middle.Processes.Shopify.Persist;
using Monster.Middle.Processes.Shopify.Workers;
using Monster.Middle.Processes.Sync.Managers;
using Monster.Middle.Processes.Sync.Persist;
using Monster.Middle.Processes.Sync.Services;
using Monster.Middle.Processes.Sync.Workers;
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
            builder.RegisterType<DefaultFormatter>().As<ILogFormatter>().InstancePerLifetimeScope();

            builder.Register(x => new NLogger(LoggerName, x.Resolve<ILogFormatter>()))
                .As<IPushLogger>()
                .InstancePerLifetimeScope();

            // TODO *** decide if it's necessary to implement this for Batch stuff!!
            //
            //.InstancePerBackgroundJobIfTrue(containerForHangFire);
            
            // System-level Persistence always uses the MonsterConfig for its Connection String
            //
            var connectionString = MonsterConfig.Settings.SystemDatabaseConnection;

            // Persistence - Master-level
            //
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
            builder.RegisterType<MasterRepository>().InstancePerLifetimeScope();

            // Persistence - Instance-level Contexts
            //
            builder.RegisterType<InstanceContext>().InstancePerLifetimeScope();
            builder.RegisterType<MiscPersistContext>().InstancePerLifetimeScope();
            builder.RegisterType<ProcessPersistContext>().InstancePerLifetimeScope();

            // Process Registrations
            //
            RegisterIdentityPlumbing(builder);
            RegisterMiscellaneous(builder);
            RegisterShopifyProcess(builder);            
            RegisterAcumaticaProcess(builder);            
            RegisterSyncProcess(builder);
            RegisterPayoutProcess(builder);

            // Crypto faculties
            //
            builder.Register<ICryptoService>(
                x => new AesCrypto(MonsterConfig.Settings.EncryptKey, MonsterConfig.Settings.EncryptIv));

            return builder;
        }

        private static void RegisterMiscellaneous(ContainerBuilder builder)
        {
            // External Services
            //
            builder.RegisterType<ExternalServiceRepository>().InstancePerLifetimeScope();

            // Hangfire 
            //
            builder.RegisterType<OneTimeJobScheduler>().InstancePerLifetimeScope();
            builder.RegisterType<RecurringJobScheduler>().InstancePerLifetimeScope();
            builder.RegisterType<JobRunner>().InstancePerLifetimeScope();
            builder.RegisterType<JobMonitoringService>().InstancePerLifetimeScope();

            // Shopify
            //
            builder.RegisterType<ShopifyTimeZoneTranslator>().InstancePerLifetimeScope();
            builder.RegisterType<ShopifyUrlService>().InstancePerLifetimeScope();
            builder.RegisterType<ShopifyPaymentGatewayService>().InstancePerLifetimeScope();

            // Acumatica
            //
            builder.RegisterType<AcumaticaUrlService>().InstancePerLifetimeScope();
            builder.RegisterType<AcumaticaTimeZoneService>().InstancePerLifetimeScope();

            // State
            //
            builder.RegisterType<StateRepository>().InstancePerLifetimeScope();

            // Logging
            //
            builder.RegisterType<ExecutionLogService>().InstancePerLifetimeScope();
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
            //
            builder.RegisterType<ShopifyBatchRepository>().InstancePerLifetimeScope();
            builder.RegisterType<ShopifyInventoryRepository>().InstancePerLifetimeScope();
            builder.RegisterType<ShopifyOrderRepository>().InstancePerLifetimeScope();
            builder.RegisterType<ShopifyPayoutRepository>().InstancePerLifetimeScope();

            builder.RegisterType<ShopifyLocationGet>().InstancePerLifetimeScope();
            builder.RegisterType<ShopifyInventoryGet>().InstancePerLifetimeScope();
            builder.RegisterType<ShopifyCustomerGet>().InstancePerLifetimeScope();
            builder.RegisterType<ShopifyOrderGet>().InstancePerLifetimeScope();
            builder.RegisterType<ShopifyTransactionGet>().InstancePerLifetimeScope();

            builder.RegisterType<ShopifyManager>().InstancePerLifetimeScope();
        }

        private static void RegisterAcumaticaProcess(ContainerBuilder builder)
        {
            // Acumatica Pull Process
            //
            builder.RegisterType<AcumaticaBatchRepository>().InstancePerLifetimeScope();
            builder.RegisterType<AcumaticaOrderRepository>().InstancePerLifetimeScope();
            builder.RegisterType<AcumaticaInventoryRepository>().InstancePerLifetimeScope();

            builder.RegisterType<AcumaticaWarehouseGet>().InstancePerLifetimeScope();
            builder.RegisterType<AcumaticaInventoryGet>().InstancePerLifetimeScope();
            builder.RegisterType<AcumaticaCustomerGet>().InstancePerLifetimeScope();
            builder.RegisterType<AcumaticaOrderGet>().InstancePerLifetimeScope();
            builder.RegisterType<AcumaticaReferenceGet>().InstancePerLifetimeScope();

            builder.RegisterType<AcumaticaManager>().InstancePerLifetimeScope();
        }

        private static void RegisterSyncProcess(ContainerBuilder builder)
        {
            // Persist
            //
            builder.RegisterType<SyncInventoryRepository>().InstancePerLifetimeScope();
            builder.RegisterType<SyncOrderRepository>().InstancePerLifetimeScope();
            builder.RegisterType<SettingsRepository>().InstancePerLifetimeScope();
            builder.RegisterType<AnalyzerRepository>().InstancePerLifetimeScope();

            // Workers 
            //
            builder.RegisterType<ShopifyInventoryPut>().InstancePerLifetimeScope();
            builder.RegisterType<ShopifyFulfillmentPut>().InstancePerLifetimeScope();
            builder.RegisterType<AcumaticaStockItemPut>().InstancePerLifetimeScope();
            builder.RegisterType<AcumaticaCustomerPut>().InstancePerLifetimeScope();
            builder.RegisterType<AcumaticaOrderPut>().InstancePerLifetimeScope();
            builder.RegisterType<AcumaticaRefundPut>().InstancePerLifetimeScope();
            builder.RegisterType<AcumaticaOrderPaymentPut>().InstancePerLifetimeScope();
            builder.RegisterType<WarehouseLocationSync>().InstancePerLifetimeScope();
            builder.RegisterType<ShopifyProductVariantPut>().InstancePerLifetimeScope();

            // Services
            //
            builder.RegisterType<CombinedRefDataService>().InstancePerLifetimeScope();
            builder.RegisterType<ConfigStatusService>().InstancePerLifetimeScope();
            builder.RegisterType<PendingActionStatusService>().InstancePerLifetimeScope();
            builder.RegisterType<OrderSyncValidationService>().InstancePerLifetimeScope();
            builder.RegisterType<FulfillmentStatusService>().InstancePerLifetimeScope();
            builder.RegisterType<AnalysisDataService>().InstancePerLifetimeScope();

            // Management Objects
            //
            builder.RegisterType<SyncManager>().InstancePerLifetimeScope();
            builder.RegisterType<ProcessDirector>().InstancePerLifetimeScope();
        }
    }
}

