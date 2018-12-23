using Autofac;
using Monster.Acumatica.Api;
using Monster.Acumatica.Config;
using Monster.Acumatica.Http;

namespace Monster.Acumatica
{
    public class AcumaticaHttpAutofac
    {
        public static void Build(ContainerBuilder builder)
        {
            builder.RegisterType<AcumaticaCredentialsConfig>();

            builder.RegisterType<AcumaticaHttpConfig>();
            builder.RegisterType<AcumaticaHttpContext>().InstancePerLifetimeScope();

            builder.RegisterType<CustomerClient>().InstancePerLifetimeScope();
            builder.RegisterType<DistributionClient>().InstancePerLifetimeScope();
            builder.RegisterType<BankClient>().InstancePerLifetimeScope();
            builder.RegisterType<SalesOrderClient>().InstancePerLifetimeScope();
            builder.RegisterType<ShipmentClient>().InstancePerLifetimeScope();
            builder.RegisterType<PaymentClient>().InstancePerLifetimeScope();
            builder.RegisterType<ReferenceClient>().InstancePerLifetimeScope();
        }
    }
}

