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
        public static void TestCompleteProcess(Guid tenantId)
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
                    var orderManager = scope.Resolve<OrderManager>();

                    /*
                    
                    // Step 1 - Synchronize Locations
                    inventoryManager.Reset(); 
                    inventoryManager.SynchronizeLocationOnly();

                    // Step 2 - Load Inventory from both systems
                    inventoryManager.SynchronizeInitial();
                    inventoryManager.LoadShopifyInventoryIntoAcumatica();
                    
                    // Step 3 - Load Orders from both systems
                    orderManager.Reset();
                    orderManager.SynchronizeInitial();
                    */

                    // Step X - Routine Synchronization Process
                    inventoryManager.SynchronizeRoutine();

                    orderManager.Reset();
                    orderManager.SynchronizeRoutine();
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

