using System;
using Autofac;
using Push.Foundation.Utilities.Logging;
using Push.Shopify.Http;
using Push.Shopify.Http.Credentials;

namespace Monster.ConsoleApp.Shopify
{
    public class ShopifyHarness
    {
        public static void InjectCredentialAndExecute(
                IContainer container, 
                IShopifyCredentials credentials,
                Action<ILifetimeScope> action)
        {
            using (var scope = container.BeginLifetimeScope())
            {
                var shopifyHttpContext = scope.Resolve<ShopifyHttpContext>();
                shopifyHttpContext.Initialize(credentials);

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

