using System;
using Autofac;
using Monster.Acumatica.Http;
using Monster.ConsoleApp.Payouts;
using Monster.Middle;
using Monster.Middle.Processes.Inventory;
using Monster.Middle.Processes.Payouts;
using Monster.Middle.Services;
using Push.Foundation.Utilities.Autofac;
using Push.Foundation.Utilities.Logging;
using Push.Shopify.Http.Credentials;


namespace Monster.ConsoleApp.Monster
{
    public class MonsterHarness
    {        
        public static void TestInventoryWorker(Guid tenantId)
        {
            using (var container = MiddleAutofac.Build())
            using (var scope = container.BeginLifetimeScope())
            {
                var logger = scope.Resolve<IPushLogger>();

                try
                {
                    var contextLoader = scope.Resolve<TenantContextLoader>();
                    contextLoader.Initialize(tenantId);

                    var worker = scope.Resolve<InventoryWorker>();
                    worker.PullLocationFromShopify();

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

