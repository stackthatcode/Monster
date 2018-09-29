using Autofac;
using Monster.Acumatica;
using Monster.Middle.Config;
using Monster.Middle.Persist.Multitenant;
using Monster.Middle.Processes.Payouts;
using Monster.Middle.Sql.Multitenant;
using Push.Foundation.Utilities.Logging;
using Push.Foundation.Web;
using Push.Shopify;
using Push.Foundation.Utilities.Security;


namespace Monster.Middle
{
    public class MiddleAutofac
    {
        public static 
                IContainer Build(
                    string connStringOverride = null,
                    string loggerName = "Monster.System")
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

            builder.Register(x => new NLogger(loggerName, x.Resolve<ILogFormatter>()))
                .As<IPushLogger>()
                .InstancePerLifetimeScope();


            // Database connectivity      
            builder.RegisterType<PersistContext>()
                .InstancePerLifetimeScope();


            // TODO *** Need to implement this!!
            //.InstancePerBackgroundJobIfTrue(containerForHangFire);

            // Persistence Repositories
            builder.RegisterType<PayoutPersistRepository>().InstancePerLifetimeScope();
            builder.RegisterType<InventoryPersistRepository>().InstancePerLifetimeScope();
            builder.RegisterType<TenantContextRepository>().InstancePerLifetimeScope();

            // Processes and Workers
            builder.RegisterType<ShopifyPayoutPullWorker>().InstancePerLifetimeScope();
            builder.RegisterType<AcumaticaPayoutPushWorkerScreen>().InstancePerLifetimeScope();
            builder.RegisterType<PayoutProcess>().InstancePerLifetimeScope();

            return builder.Build();
        }
    }
}

