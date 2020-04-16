using Autofac;
using Monster.Middle;
using Push.Foundation.Web;

namespace Monster.ConsoleApp
{
    public class ConsoleAppAutofac
    {
        public static IContainer BuildContainer()
        {
            var builder = new ContainerBuilder();
            
            MiddleAutofac.Build(builder);
            FoundationWebAutofac.RegisterOwinAuthentication(builder);
            return builder.Build();
        }
    }
}
