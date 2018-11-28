using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Web.Mvc;
using Autofac;
using Autofac.Integration.Mvc;
//using RequestActivityIdLogFormatter = Enable.Web.Plumbing.RequestActivityIdLogFormatter;

namespace Enable.Web
{
    public class WebAutofac
    {
        public static IContainer Build()
        {
            var builder = new ContainerBuilder();

            // Push.Foundation.Web relies on consumers to supply Key and IV for its Encryption Service
            Push.Foundation.Web.FoundationWebAutofac.Build(builder);

            Enable.Middleware.MiddlewareAutofac.Build(builder);

            // Logging
            var loggerName = "Enable.Web";

            builder.RegisterType<RequestActivityIdLogFormatter>()
                    .As<ILogFormatter>()
                    .InstancePerLifetimeScope();

            builder.Register(x => new NLogger(loggerName, x.Resolve<ILogFormatter>()))
                    .As<IPushLogger>()
                    .InstancePerLifetimeScope();
            
            // Database connection string...
            var connectionString =
                    ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

            // ... and register configuration
            builder
                .Register(ctx =>
                {
                    var connectionstring = connectionString;
                    var connection = new SqlConnection(connectionstring);
                    connection.Open();
                    return connection;
                })
                .As<SqlConnection>()
                .As<DbConnection>()
                .As<IDbConnection>()
                .InstancePerLifetimeScope();

            // Critical piece for all database infrastructure to work smoothly
            builder.RegisterType<ConnectionWrapper>().InstancePerLifetimeScope();
            builder.RegisterType<EnableDbContext>().InstancePerLifetimeScope();

            // Controller registration
            builder.RegisterType<ErrorController>();


            var container = builder.Build();
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));
            return container;
        }
    }
}

