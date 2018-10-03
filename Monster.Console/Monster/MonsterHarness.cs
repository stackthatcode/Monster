using System;
using Autofac;
using Monster.Middle;
using Monster.Middle.Processes.Inventory;
using Monster.Middle.Services;
using Push.Foundation.Utilities.Logging;


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
                    var contextLoader = scope.Resolve<TenantContext>();
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

