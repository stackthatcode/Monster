using System;
using Autofac;
using Monster.ConsoleApp.Testing.Feeder;
using Monster.Middle;
using Monster.Middle.Misc.Hangfire;
using Push.Foundation.Web;

namespace Monster.ConsoleApp
{
    public class ConsoleAppAutofac
    {
        public static IContainer BuildContainer()
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<ShopifyDataFeeder>();

            MiddleAutofac.Build(builder);
            FoundationWebAutofac.RegisterOwinAuthentication(builder);
            return builder.Build();
        }
    }
}
