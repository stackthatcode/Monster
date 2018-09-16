﻿using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using Autofac;
using Monster.Acumatica;
using Monster.Middle;
using Push.Foundation.Utilities.Helpers;
using Push.Foundation.Utilities.Logging;
using Push.Foundation.Web;
using Push.Shopify;

namespace Monster.ConsoleApp
{
    public class ConsoleAutofac
    {
        public static IContainer Build(
                bool containerForHangFire = false,
                string connectionStringOverride = null)
        {
            var builder = new ContainerBuilder();

            // Logger configuration
            const string loggerName = "Monster.Console";
            
            builder.RegisterType<DefaultFormatter>()
                    .As<ILogFormatter>();
            // .InstancePerBackgroundJobIfTrue(containerForHangFire);

            builder.Register(x => new NLogger(loggerName, x.Resolve<ILogFormatter>()))
                    .As<IPushLogger>();
            // .InstancePerBackgroundJobIfTrue(containerForHangFire);

            // Data configuration
            var connectionString =
                !connectionStringOverride.IsNullOrEmpty() 
                    
                    ? connectionStringOverride 
                    
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
            //.InstancePerBackgroundJobIfTrue(containerForHangFire);


            // Register external assemblies
            FoundationWebAutofac.Build(builder);
            AcumaticaHttpAutofac.Build(builder);
            ShopifyApiAutofac.Build(builder);
            MiddleAutofac.Build(builder);

            return builder.Build();
        }
    }
}

