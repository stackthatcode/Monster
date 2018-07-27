using Autofac;
using Monster.Middle.EF;
using Monster.Middle.Workers;
using Monster.Middle.Workers.Permutation;

namespace Monster.Middle
{
    public class MiddleAutofac
    {
        public static void Build(ContainerBuilder builder)
        {
            builder.RegisterType<BundleDataContext>().InstancePerLifetimeScope();
            builder.RegisterType<Repository>().InstancePerLifetimeScope();
            builder.RegisterType<PermutationWorker>();
        }
    }
}

