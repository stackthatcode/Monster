using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Web.Mvc;
using Autofac;
using Autofac.Integration.Mvc;
using Monster.Web.Controllers;
using Push.Foundation.Utilities.Logging;
using RequestActivityIdLogFormatter 
        = Monster.Web.Plumbing.RequestActivityIdLogFormatter;


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
            
            // Database Connection for ... OWIN stuff...?
            //ConfigureDatabaseConnection(builder);

            // ASP.NET MVC Controller registration
            builder.RegisterType<ConfigController>();
            

            var container = builder.Build();
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));
            return container;
        }


        // TODO - figure out who needs this...?
        public static void ConfigureDatabaseConnection(ContainerBuilder builder)
        {
            // Database connection string...
            var connectionString =
                ConfigurationManager
                    .ConnectionStrings["DefaultConnection"]
                    .ConnectionString;
            
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
        }
    }
}
