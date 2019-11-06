using System;
using Autofac;
using Monster.Acumatica.Api;
using Monster.Acumatica.Http;
using Monster.ConsoleApp.Testing.Feeder;
using Monster.Middle.Misc.Acumatica;
using Monster.Middle.Persist.Instance;
using Monster.Middle.Processes.Acumatica.Workers;
using Monster.Middle.Processes.Shopify.Workers;
using Monster.Middle.Processes.Sync.Workers;
using Push.Foundation.Utilities.Helpers;

namespace Monster.ConsoleApp.Testing
{
    public class MoreTestingStuff
    {
        private static Guid TestInstanceId = Guid.Parse("51AA413D-E679-4F38-BA47-68129B3F9212");

        public static void RunAcumaticaOrderSync()
        {
            Console.WriteLine(
                Environment.NewLine + "Enter Shopify Order ID (Default ID: 1838808563756)");
            var shopifyOrderId = Console.ReadLine().IsNullOrEmptyAlt("1838808563756").ToLong();

            AutofacRunner.RunInLifetimeScope(scope =>
            {
                var instanceContext = scope.Resolve<InstanceContext>();
                var acumaticaContext = scope.Resolve<AcumaticaHttpContext>();
                var orderSync = scope.Resolve<AcumaticaOrderPut>();

                instanceContext.Initialize(TestInstanceId);
                acumaticaContext.SessionRun(() => orderSync.RunOrder(shopifyOrderId));
            });
        }

        public static void RunAcumaticaCustomerGet()
        {
            AutofacRunner.RunInLifetimeScope(scope =>
            {
                var instanceContext = scope.Resolve<InstanceContext>();
                var acumaticaContext = scope.Resolve<AcumaticaHttpContext>();
                var acumaticaCustomerGet = scope.Resolve<AcumaticaCustomerGet>();

                instanceContext.Initialize(TestInstanceId);
                acumaticaContext.SessionRun(() =>
                {
                    acumaticaCustomerGet.RunAutomatic();
                });
            });
        }

        public static void RunAcumaticaOrderGet()
        {
            AutofacRunner.RunInLifetimeScope(scope =>
            {
                var instanceContext = scope.Resolve<InstanceContext>();
                var acumaticaOrderGet = scope.Resolve<AcumaticaOrderGet>();

                instanceContext.Initialize(TestInstanceId);
                acumaticaOrderGet.RunAutomatic();
            });
        }

        public static void RunAcumaticaPaymentGet()
        {
            AutofacRunner.RunInLifetimeScope(scope =>
            {
                var instanceContext = scope.Resolve<InstanceContext>();
                var acumaticaContext = scope.Resolve<AcumaticaHttpContext>();
                var paymentClient = scope.Resolve<PaymentClient>();

                instanceContext.Initialize(TestInstanceId);
                acumaticaContext.SessionRun(() =>
                {
                    var result = paymentClient.RetrievePayment("000014", "PMT", "ApplicationHistory"); 
                    
                    // "DocumentsToApply");
                });
            });
        }



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

        public static void RunShopifyOrderGet()
        {
            AutofacRunner.RunInLifetimeScope(scope =>
            {
                var instanceContext = scope.Resolve<InstanceContext>();
                var shopifyOrderGet = scope.Resolve<ShopifyOrderGet>();

                instanceContext.Initialize(TestInstanceId);
                shopifyOrderGet.RunAutomatic();
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

        public static void RunShopifyOrderGetById()
        {
            Console.WriteLine(
                Environment.NewLine + "Enter Shopify Order ID (Default ID: 1838808563756)");

            var shopifyOrderId = Console.ReadLine().IsNullOrEmptyAlt("1838808563756	").ToLong();

            AutofacRunner.RunInLifetimeScope(scope =>
            {
                var instanceContext = scope.Resolve<InstanceContext>();
                instanceContext.Initialize(TestInstanceId);

                var shopifyOrderGet = scope.Resolve<ShopifyOrderGet>();
                var order = shopifyOrderGet.Run(shopifyOrderId).order;
                
                //
                // *** TODO - emit the computations
                //
            });
        }

    }
}
