using System;
using Autofac;
using Monster.Acumatica.Api;
using Monster.Acumatica.Http;
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
                    var tenantContext = scope.Resolve<TenantContext>();
                    tenantContext.Initialize(tenantId);

                    var worker = scope.Resolve<InventoryWorker>();
                    worker.PullLocationsFromShopify();

                    var acumaticaContext = scope.Resolve<AcumaticaHttpContext>();
                    acumaticaContext.Begin();

                    worker.PullWarehousesFromAcumatica();

                    worker.FlagDifferencesAndExceptions();

                    // Next Pull Posting Class from Acumatica

                    // Next Create Locations in Acumatica

                    // Next Pull Locations from Acumatica

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

