using System;
using System.Linq;
using Autofac;
using Monster.Middle;
using Monster.Middle.Processes.Acumatica;
using Monster.Middle.Processes.Inventory;
using Monster.Middle.Processes.Shopify;
using Monster.Middle.Processes.Sync.Inventory;
using Monster.Middle.Processes.Sync.Orders;
using Monster.Middle.Services;
using Push.Foundation.Utilities.Helpers;
using Push.Foundation.Utilities.Logging;
using Push.Shopify.Api;
using Push.Shopify.Api.Order;


namespace Monster.ConsoleApp.Monster
{
    public class MonsterHarness
    {
        public static void LoadWarehouses(Guid tenantId)
        {
            ExecuteInScope(tenantId, scope =>
            {
                var tenantContext = scope.Resolve<TenantContext>();
                tenantContext.Initialize(tenantId);

                var acumaticaManager = scope.Resolve<AcumaticaManager>();
                var shopifyManager = scope.Resolve<ShopifyManager>();
                var inventoryManager = scope.Resolve<InventoryManager>();

                // Step 1 - Pull Locations and Warehouses
                acumaticaManager.PullWarehouses();
                shopifyManager.PullLocations();


                // Step 2 - Synchronize Locations and Warehouses
                inventoryManager.SynchronizeLocationOnly();

                if (!inventoryManager.LocationStatusCheck())
                {
                    Console.WriteLine("Aborting process - Warehouse/Locations not properly synched");
                }
            });
        }

        public static void LoadInventory(Guid tenantId)
        {
            ExecuteInScope(tenantId, scope =>
            {
                var inventoryManager = scope.Resolve<InventoryManager>();
                
                // Step 1 - Load Shopify Inventory into Acumatica as baseline
                inventoryManager.PushShopifyInventoryIntoAcumatica();

                // *** PAUSE TO ALLOW ACUMATICA CACHE TO REFRESH ***
                Console.WriteLine("Need Acumatica cache to update - hit enter to continue...");
                Console.ReadLine();


                // Step 2 - Load Acumatica Inventory into Shopify
                inventoryManager.PushAcumaticaInventoryIntoShopify();
            });
        }

        public static void RoutineShopifyPull(Guid tenantId)
        {
            ExecuteInScope(tenantId, scope =>
            {
                var shopifyManager = scope.Resolve<ShopifyManager>();
                shopifyManager.PullOrdersAndCustomers();
            });
        }

        public static void RoutineAcumaticaPull(Guid tenantId)
        {
            ExecuteInScope(tenantId, scope =>
            {
                var acumaticaManager = scope.Resolve<AcumaticaManager>();
                acumaticaManager.PullInventory();
                acumaticaManager.PullCustomerAndOrdersAndShipments();
            });
        }

        public static void RoutineSynchronization(Guid tenantId)
        {
            ExecuteInScope(tenantId, scope =>
            {

                var inventoryManager = scope.Resolve<InventoryManager>();
                var orderManager = scope.Resolve<OrderManager>();

                // Step 1 - Load Acumatica Inventory into Shopify
                inventoryManager.PushAcumaticaInventoryIntoShopify();

                // Step 2 (optional) - Load Products into Acumatica
                orderManager.LoadShopifyProductsIntoAcumatica();

                // Step 3 - Load Orders, Refunds, Payments and Shipments
                orderManager.RoutineOrdersSync();
            });
        }

        public static void LoadShopifyOrderNbr(Guid tenantId)
        {
            Console.WriteLine("Enter Shopify Order Number");
            var number = Console.ReadLine().ToLong();

            using (var container = MiddleAutofac.Build())
            using (var scope = container.BeginLifetimeScope())
            {
                var tenantContext = scope.Resolve<TenantContext>();
                tenantContext.Initialize(tenantId);

                var logger = scope.Resolve<IPushLogger>();

                try
                {
                    var api = scope.Resolve<OrderApi>();
                    var json = api.RetrieveByName(number);

                    var orders = json.DeserializeToOrderList();
                    var order = orders.orders.FirstOrDefault();
                }
                catch (Exception ex)
                {
                    logger.Error(ex);
                    throw;
                }
            } 
        }


        public static void ExecuteInScope(Guid tenantId, Action<ILifetimeScope> task)
        {
            using (var container = MiddleAutofac.Build())
            using (var scope = container.BeginLifetimeScope())
            {
                var logger = scope.Resolve<IPushLogger>();

                try
                {
                    var tenantContext = scope.Resolve<TenantContext>();
                    tenantContext.Initialize(tenantId);

                    task(scope);
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

