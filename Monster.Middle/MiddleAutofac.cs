using Autofac;
using Monster.Middle.EF;
using Monster.Middle.Processes.Payouts;

namespace Monster.Middle
{
    public class MiddleAutofac
    {
        public static void Build(ContainerBuilder builder)
        {
            builder.RegisterType<MonsterDataContext>().InstancePerLifetimeScope();
            builder.RegisterType<PayoutImportRepository>().InstancePerLifetimeScope();

            builder.RegisterType<ShopifyPayoutPullWorker>().InstancePerLifetimeScope();
            builder.RegisterType<AcumaticaPayoutPushWorker>().InstancePerLifetimeScope();
            builder.RegisterType<PayoutProcess>().InstancePerLifetimeScope();
        }
    }
}

