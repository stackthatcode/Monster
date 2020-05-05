using Autofac;
using Monster.Middle;
using Push.Foundation.Web;

namespace Monster.ConsoleApp
{
    public class AutofacBuilder
    {
        public static IContainer BuildContainer(bool sentryEnabled = false)
        {
            var builder = new ContainerBuilder();
            
            MiddleAutofac.Build(builder, sentryEnabled);

            // TODO: Why is this necessary? Can't we just use a stub object i.e. it's not getting invoked?
            //
            FoundationWebAutofac.RegisterOwinAuthentication(builder);

            return builder.Build();
        }
    }
}
