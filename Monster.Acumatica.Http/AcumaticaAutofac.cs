﻿using Autofac;
using Monster.Acumatica.Api;
using Monster.Acumatica.Config;
using Monster.Acumatica.Http;

namespace Monster.Acumatica
{
    public class AcumaticaHttpAutofac
    {
        public static void Build(ContainerBuilder builder)
        {
            builder.RegisterType<AcumaticaHttpConfig>();
            builder.RegisterType<AcumaticaCredentialsConfig>();

            builder.RegisterType<AcumaticaHttpContext>().InstancePerLifetimeScope();

            builder.RegisterType<CustomerClient>().InstancePerLifetimeScope();
            builder.RegisterType<DistributionClient>().InstancePerLifetimeScope();
            builder.RegisterType<BankClient>().InstancePerLifetimeScope();
        }
    }
}

