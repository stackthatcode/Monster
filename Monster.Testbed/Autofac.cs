using System;
using Autofac;
using Monster.Middle;
using Monster.Testbed.ShopifyFeeder;
using Push.Foundation.Utilities.Logging;
using Push.Foundation.Web;

namespace Monster.Testbed
{
    public class Autofac
    {
        public static IContainer BuildContainer()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<ShopifyDataFeeder>();

            MiddleAutofac.Build(builder);
            FoundationWebAutofac.RegisterOwinAuthentication(builder);

            return builder.Build();
        }


        public static void RunInScope(Action<ILifetimeScope> action)
        {
            var container = Autofac.BuildContainer();
            //HangFireConfig.ConfigureStorage();

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
