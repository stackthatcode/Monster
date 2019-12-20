using System;
using Autofac;
using Monster.Middle;
using Push.Foundation.Utilities.Logging;
using Push.Foundation.Web;

namespace Monster.ConsoleApp
{
    public class AutofacRunner
    {
        public static void RunInLifetimeScope(
            Action<ILifetimeScope> action, Action<ContainerBuilder> builderPreExec = null)
        {
            var builder = new ContainerBuilder();
            builderPreExec?.Invoke(builder);
            MiddleAutofac.Build(builder);
            FoundationWebAutofac.RegisterOwinAuthentication(builder);

            var container = builder.Build();
            using (var scope = container.BeginLifetimeScope())
            {
                var logger = scope.Resolve<IPushLogger>();

                try
                {
                    action(scope);
                }
                catch (Exception ex)
                {
                    logger.Error(ex);
                }
            }
        }
    }
}

