using System;
using Autofac;
using Monster.Middle;
using Monster.Middle.Misc.Hangfire;
using Push.Foundation.Utilities.Logging;
using Push.Foundation.Web;

namespace Monster.ConsoleApp
{
    public class AutofacRunner
    {
        public static void RunInScope(Action<ILifetimeScope> action)
        {
            var container =  ConsoleAppAutofac.BuildContainer();
            HangFireConfig.ConfigureStorage();

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

