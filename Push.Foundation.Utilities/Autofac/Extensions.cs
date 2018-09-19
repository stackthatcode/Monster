using System;
using Autofac;
using Push.Foundation.Utilities.Logging;

namespace Push.Foundation.Utilities.Autofac
{
    public static class Extensions
    {
        public static void 
                ExecuteInLifetimeScope(
                    this IContainer container, 
                    Action<ILifetimeScope> action)
        {
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
