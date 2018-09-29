using Autofac;
using Monster.Acumatica.Http;
using Monster.Middle;
using Monster.Middle.Processes.Inventory;
using Monster.Middle.Processes.Payouts;
using Push.Foundation.Utilities.Autofac;
using Push.Shopify.Http.Credentials;


namespace Monster.ConsoleApp.Monster
{
    public class MonsterHarness
    {
        public const string DefaultLoggerName = "Monster.Payouts";

        //
        // NOTE => The security settings will have to be inferred by the task identifiers
        // ... and the configuration data pulled on a per-tenant basis from 
        // ... secure storage. For now, we stub away!
        //
        

        public static void TestInventory(
                PrivateAppCredentials shopifyCredentials,
                AcumaticaCredentials acumaticaCredentials,
                PayoutConfig payoutConfig)
        {
            using (var container = MiddleAutofac.Build())
            {
                container.RunInLifetimeScope(
                    scope =>
                    {
                        var worker = scope.Resolve<InventoryWorker>();
                        
                    });
            }
        }

    }
}
