using Autofac;
using Monster.Acumatica;
using Monster.Acumatica.BankImportApi;
using Monster.Middle.Config;
using Monster.Middle.Persist.Multitenant;
using Monster.Middle.Persist.Sys;
using Monster.Middle.Processes.Inventory;
using Monster.Middle.Processes.Inventory.Workers;
using Monster.Middle.Processes.Payouts;
using Monster.Middle.Services;
using Push.Foundation.Utilities.Logging;
using Push.Foundation.Web;
using Push.Shopify;
using Push.Foundation.Utilities.Security;
using AcumaticaWarehousePull = Monster.Middle.Processes.Inventory.AcumaticaWarehousePull;


namespace Monster.Middle
{
    public class MiddleAutofac
    {
        public static string LoggerName = "Monster.System";

        public static IContainer Build()
        {
            var builder = new ContainerBuilder();

            // Register external assemblies
            FoundationWebAutofac.Build(builder);


            // Register Acumatica library and inject settings
            AcumaticaHttpAutofac.Build(builder);
            

            // Register Shopify library and inject settings
            ShopifyApiAutofac.Build(builder);
            

            // Cryptographic
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
            builder.RegisterType<PayoutRepository>().InstancePerLifetimeScope();
            builder.RegisterType<BatchStateRepository>().InstancePerLifetimeScope();
            builder.RegisterType<LocationRepository>().InstancePerLifetimeScope();
            builder.RegisterType<InventoryRepository>().InstancePerLifetimeScope();
            builder.RegisterType<TenantRepository>().InstancePerLifetimeScope();

            // Tenant Context
            builder.RegisterType<TenantContext>().InstancePerLifetimeScope();

            // Payout Processes
            builder.RegisterType<Screen>().InstancePerLifetimeScope();
            builder.RegisterType<BankImportService>().InstancePerLifetimeScope();
            builder.RegisterType<ShopifyPayoutPullWorker>().InstancePerLifetimeScope();
            builder.RegisterType<PayoutProcess>().InstancePerLifetimeScope();
            
            // Inventory
            builder.RegisterType<AcumaticaWarehousePull>().InstancePerLifetimeScope();
            builder.RegisterType<AcumaticaInventoryPull>().InstancePerLifetimeScope();
            builder.RegisterType<ShopifyLocationWorker>().InstancePerLifetimeScope();
            builder.RegisterType<ShopifyInventoryPullWorker>().InstancePerLifetimeScope();
            builder.RegisterType<InventoryManager>().InstancePerLifetimeScope();

            return builder.Build();
        }
    }
}

