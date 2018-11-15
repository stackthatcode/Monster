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
        
        public static void InitialLoad(Guid tenantId)
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

                    // Step 1 - Synchronize Locations
                    inventoryManager.Reset();
                    inventoryManager.SynchronizeLocationOnly();

                    // Step 2 - Inventory baseline, use Shopify Inventory as starting point
                    inventoryManager.SynchronizeInitial();
                    inventoryManager.LoadShopifyInventoryIntoAcumatica();
                    
                    // *** PAUSE TO ALLOW ACUMATICA CACHE TO REFRESH ***
                    Console.WriteLine("Need Acumatica cache to update - hit enter to continue...");
                    Console.ReadLine();

                    // Step 3 - Load Acumatica into Shopify
                    inventoryManager.SynchronizeShopifyInitial();
                    
                    // Step 4 - Initial Order Synchronization
                    orderManager.Reset();
                    orderManager.SynchronizeInitial();
                }
                catch (Exception ex)
                {
                    logger.Error(ex);
                    throw;
                }
            }
        }

        public static void RoutineExecution(Guid tenantId)
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

                    inventoryManager.SynchronizeShopifyRoutine();
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

