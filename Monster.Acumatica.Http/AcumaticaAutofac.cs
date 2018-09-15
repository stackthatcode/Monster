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
            builder.RegisterType<AcumaticaHttpSettings>();
            builder.RegisterType<AcumaticaApiFactory>();
            builder.RegisterType<AcumaticaHttpClientFactory>();
            builder.RegisterType<SessionContainer>()
                    .InstancePerLifetimeScope();

            builder.RegisterType<SessionRepository>();
            builder.RegisterType<CustomerRepository>();
            builder.RegisterType<InventoryRepository>();
            builder.RegisterType<BankRepository>();
        }
    }
}

