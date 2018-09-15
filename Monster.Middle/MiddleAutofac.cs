using Autofac;
using Monster.Middle.EF;

namespace Monster.Middle
{
    public class MiddleAutofac
    {
        public static void Build(ContainerBuilder builder)
        {
            builder.RegisterType<MonsterDataContext>().InstancePerLifetimeScope();
            builder.RegisterType<Repository>().InstancePerLifetimeScope();
        }
    }
}

