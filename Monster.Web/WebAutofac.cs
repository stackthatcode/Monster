using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Web.Mvc;
using Autofac;
using Autofac.Integration.Mvc;
using Monster.Middle.Config;
using Monster.Web.Controllers;
using Push.Foundation.Utilities.Logging;
using Push.Foundation.Utilities.Security;
using Push.Shopify.Config;
using RequestActivityIdLogFormatter = Monster.Web.Plumbing.RequestActivityIdLogFormatter;


namespace Monster.Web
{
    public class WebAutofac
    {
        public static IContainer Build()
        {
            var builder = new ContainerBuilder();

            // Dependent component registrations
            Push.Foundation.Web.FoundationWebAutofac.Build(builder);
            Middle.MiddleAutofac.Build(builder);

            // Logging
            var loggerName = "Monster.Web"; // TODO - make this a singleton/static value

            builder.RegisterType<RequestActivityIdLogFormatter>()
                    .As<ILogFormatter>()
                    .InstancePerLifetimeScope();

            builder.Register(x => new NLogger(loggerName, x.Resolve<ILogFormatter>()))
                    .As<IPushLogger>()
                    .InstancePerLifetimeScope();

            // Database Connection for OWIN stuff
            var systemDbConnection = MonsterConfig.Settings.SystemDatabaseConnection;
            builder
                .Register(ctx =>
                {
                    var connection = new SqlConnection(systemDbConnection);
                    connection.Open();
                    return connection;
                })
                .As<SqlConnection>()
                .As<DbConnection>()
                .As<IDbConnection>()
                .InstancePerLifetimeScope();


            // ASP.NET MVC Controller registration
            builder.RegisterType<ShopifyAuthController>();
            builder.RegisterType<ConfigController>();

            // HMAC Service - used to hash Auth Code and verify Shopify response
            builder.Register(x => new HmacCryptoService(
                        ShopifyCredentialsConfig.Settings.ApiSecret));

            var container = builder.Build();
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));
            return container;
        }


        public static void ConfigureDatabaseConnection(ContainerBuilder builder)
        {
            // Database connection string...
            var systemDbConnection 
                    = MonsterConfig.Settings.SystemDatabaseConnection;

            // ... and register configuration

        }
    }
}
