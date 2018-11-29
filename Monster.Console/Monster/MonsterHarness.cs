﻿using System;
using Autofac;
using Monster.Middle;
using Monster.Middle.Processes.Acumatica;
using Monster.Middle.Processes.Acumatica.Persist;
using Monster.Middle.Processes.Shopify;
using Monster.Middle.Processes.Shopify.Persist;
using Monster.Middle.Processes.Sync.Inventory;
using Monster.Middle.Processes.Sync.Orders;
using Monster.Middle.Services;
using Push.Foundation.Utilities.Logging;


namespace Monster.ConsoleApp.Monster
{
    public class MonsterHarness
    {
        public static void ResetBatchStates(Guid tenantId)
        {
            ExecuteInScope(tenantId, scope =>
            {
                var shopifyBatchRepository = scope.Resolve<ShopifyBatchRepository>();
                var acumaticaBatchRepository = scope.Resolve<AcumaticaBatchRepository>();

                shopifyBatchRepository.Reset();
                acumaticaBatchRepository.Reset();
            });
        }

        public static void LoadWarehouses(Guid tenantId)
        {
            ExecuteInScope(tenantId, scope =>
            {
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
                var acumaticaManager = scope.Resolve<AcumaticaManager>();
                var shopifyManager = scope.Resolve<ShopifyManager>();

                // Step 1 - Pull Shopify Inventory
                shopifyManager.PullInventory();

                // Step 2 - Pull Acumatica Inventory
                acumaticaManager.PullInventory();

                // Step 3 - Load Shopify Inventory into Acumatica as baseline
                inventoryManager.PushShopifyInventoryIntoAcumatica();


                // *** PAUSE TO ALLOW ACUMATICA CACHE TO REFRESH ***
                Console.WriteLine("Need Acumatica cache to update - hit enter to continue...");
                Console.ReadLine();


                // Step 4 - Pull Acumatica Inventory
                acumaticaManager.PullInventory();

                // Step 5 - Load Acumatica Inventory into Shopify
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
        
        public static void ExecuteInScope(Guid tenantId, Action<ILifetimeScope> task)
        {
            var builder = new ContainerBuilder();
            using (var container = MiddleAutofac.Build(builder).Build())
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

