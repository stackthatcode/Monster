using System;
using Autofac;
using Monster.Acumatica.Http;
using Monster.ConsoleApp.Testing.Feeder;
using Monster.Middle.Persist.Instance;
using Monster.Middle.Processes.Acumatica.Workers;
using Monster.Middle.Processes.Shopify.Workers;
using Monster.Middle.Processes.Sync.Workers;
using Monster.Middle.Utility;
using Push.Foundation.Utilities.Helpers;

namespace Monster.ConsoleApp.Testing
{
    public class MoreTestingStuff
    {
        private static Guid TestInstanceId = Guid.Parse("51AA413D-E679-4F38-BA47-68129B3F9212");

        public static void RunShopifyOrderFeeder()
        {
            AutofacRunner.RunInLifetimeScope(
                scope =>
                {
                    var feeder = scope.Resolve<ShopifyDataFeeder>();
                    feeder.Run();
                },
                builder =>
                {
                    builder.RegisterType<ShopifyDataFeeder>().InstancePerLifetimeScope();
                });
        }

        public static void RunAcumaticaOrderSync()
        {
            Console.WriteLine(
                Environment.NewLine + "Enter Shopify Order ID (Default ID: 1778846826540)");
            var shopifyOrderId = Console.ReadLine().IsNullOrEmptyAlt("1778846826540").ToLong();

            AutofacRunner.RunInLifetimeScope(scope =>
            {
                var instanceContext = scope.Resolve<InstanceContext>();

                var acumaticaOrderPull = scope.Resolve<AcumaticaOrderGet>();
                var shopifyOrderPull = scope.Resolve<ShopifyOrderGet>();
                var acumaticaContext = scope.Resolve<AcumaticaHttpContext>();
                var orderSync = scope.Resolve<AcumaticaOrderPut>();

                instanceContext.Initialize(TestInstanceId);

                //acumaticaContext.SessionRun(() => acumaticaOrderPull.RunAutomatic());
                //shopifyOrderPull.RunAutomatic();
                acumaticaContext.SessionRun(() => orderSync.RunOrder(shopifyOrderId));
            });
        }

        public static void RunShopifyOrderTimezoneTest()
        {
            Console.WriteLine(
                Environment.NewLine + "Enter Shopify Order ID (Default ID: 1778846826540)");
            var shopifyOrderId = Console.ReadLine().IsNullOrEmptyAlt("1778846826540").ToLong();

            AutofacRunner.RunInLifetimeScope(scope =>
            {
                var instanceContext = scope.Resolve<InstanceContext>();
                instanceContext.Initialize(TestInstanceId);

                var shopifyOrderGet = scope.Resolve<ShopifyOrderGet>();
                var acumaticaTimeZone = scope.Resolve<AcumaticaTimeZoneService>();

                var order = shopifyOrderGet.Run(shopifyOrderId).order;

                var createdAt = order.created_at;
                var createdAtUtc = order.created_at.ToUniversalTime();

                var acumaticaTime = acumaticaTimeZone.ToAcumaticaTimeZone(createdAtUtc.DateTime);
            });
        }
    }
}
