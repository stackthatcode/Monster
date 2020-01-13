using System;
using Autofac;
using Push.Foundation.Utilities.Logging;

namespace Monster.ConsoleApp
{
    public class AutofacRunner
    {
        public static void RunInScope(Action<ILifetimeScope> action)
        {
            var container =  ConsoleAppAutofac.BuildContainer();
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

