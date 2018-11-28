using System;
using Autofac;
using Monster.Middle;
using Push.Foundation.Utilities.Logging;

namespace Push.Foundation
{
    public class Helper
    {
        public static void RunInLifetimeScope(Action<ILifetimeScope> action)
        {
            var builder = new ContainerBuilder();
            using (var container = MiddleAutofac.Build(builder).Build())
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
                    throw;
                }
            }
        }
    }
}
