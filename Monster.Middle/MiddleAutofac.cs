using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using Autofac;
using Monster.Acumatica;
using Monster.Acumatica.BankImportApi;
using Monster.Middle.EF;
using Monster.Middle.Processes.Payouts;
using Push.Foundation.Utilities.Logging;
using Push.Foundation.Web;
using Push.Shopify;
using Push.Foundation.Utilities.Helpers;

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
            AcumaticaHttpAutofac.Build(builder);
            ShopifyApiAutofac.Build(builder);

            
            builder.RegisterType<DefaultFormatter>()
                .As<ILogFormatter>();
            // .InstancePerBackgroundJobIfTrue(containerForHangFire);

            builder.Register(x => new NLogger(loggerName, x.Resolve<ILogFormatter>()))
                .As<IPushLogger>();
            // .InstancePerBackgroundJobIfTrue(containerForHangFire);


            // Data configuration
            var connectionString =
                !connStringOverride.IsNullOrEmpty()

                    ? connStringOverride

                    : ConfigurationManager
                        .ConnectionStrings["DefaultConnection"]
                        .ConnectionString;

            // Database connection registration
            builder
                .Register(ctx =>
                {
                    var connection = new SqlConnection(connectionString);
                    connection.Open();
                    return connection;
                })
                .As<SqlConnection>()
                .As<DbConnection>()
                .As<IDbConnection>();

            // TODO *** Need to implement this!!
            //.InstancePerBackgroundJobIfTrue(containerForHangFire);

            // SQL Persistence Repositories
            builder.RegisterType<MonsterDataContext>().InstancePerLifetimeScope();
            builder.RegisterType<PayoutPersistRepository>().InstancePerLifetimeScope();
            builder.RegisterType<InventoryPersistRepository>().InstancePerLifetimeScope();

            // Acumatica Bank Import API screen
            builder.RegisterType<Screen>().InstancePerLifetimeScope();

            builder.RegisterType<ShopifyPayoutPullWorker>().InstancePerLifetimeScope();
            builder.RegisterType<AcumaticaPayoutPushWorkerScreen>().InstancePerLifetimeScope();
            builder.RegisterType<PayoutProcess>().InstancePerLifetimeScope();

            return builder.Build();
        }
    }
}

