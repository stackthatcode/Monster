using System;
using Autofac;
using Monster.Middle;
using Monster.Middle.Processes.Inventory;
using Monster.Middle.Processes.Orders;
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
                    var tenantContext = scope.Resolve<TenantContext>();
                    tenantContext.Initialize(tenantId);

                    var inventoryManager = scope.Resolve<InventoryManager>();
                    inventoryManager.Baseline();
                    inventoryManager.Incremental();

                    var orderManager = scope.Resolve<OrderManager>();
                    orderManager.Baseline();
                    orderManager.Incremental();
                    //orderManager.SingleOrderPush(666924580962);

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

