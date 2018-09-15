using Autofac;
using Monster.Middle.EF;
using Monster.Middle.Workers;

namespace Monster.Middle
{
    public class MiddleAutofac
    {
        public static void Build(ContainerBuilder builder)
        {
            builder.RegisterType<MonsterDataContext>().InstancePerLifetimeScope();
            builder.RegisterType<PayoutImportRepository>().InstancePerLifetimeScope();

            builder.RegisterType<ShopifyPayoutPull>().InstancePerLifetimeScope();
            builder.RegisterType<AcumaticaPayoutPush>().InstancePerLifetimeScope();
        }
    }
}

