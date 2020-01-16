﻿using System;
using Autofac;
using Monster.Acumatica.Api;
using Monster.Acumatica.Api.SalesOrder;
using Monster.Acumatica.Http;
using Monster.ConsoleApp.Testing.Feeder;
using Monster.Middle.Misc.Acumatica;
using Monster.Middle.Persist.Instance;
using Monster.Middle.Processes.Acumatica.Persist;
using Monster.Middle.Processes.Acumatica.Workers;
using Monster.Middle.Processes.Shopify.Workers;
using Monster.Middle.Processes.Sync.Workers;
using Newtonsoft.Json;
using Push.Foundation.Utilities.General;
using Push.Foundation.Utilities.Helpers;
using Push.Foundation.Utilities.Json;
using Push.Foundation.Utilities.Logging;
using Push.Shopify.Api;


namespace Monster.ConsoleApp.Testing
{
    public class MoreTestingStuff
    {
        public static long SolicitShopifyOrderId()
        {
            Console.WriteLine(Environment.NewLine + "Enter Shopify Order Id (Default Id: 1840328409132)");
            return Console.ReadLine().IsNullOrEmptyAlt("1840328409132").ToLong();
        }

        public static long SolicitShopifyFulfillmentId(long defaultId)
        {
            Console.WriteLine(Environment.NewLine + $"Enter Shopify Fulfillment Id (Default Id: {defaultId})");
            return Console.ReadLine().IsNullOrEmptyAlt(defaultId.ToString()).ToLong();
        }

        public static void RunAcumaticaOrderSync()
        {
            Console.WriteLine(
                Environment.NewLine + "Enter Shopify Order ID (Default ID: 1840328409132)");
            var shopifyOrderId = Console.ReadLine().IsNullOrEmptyAlt("1840328409132").ToLong();

            AutofacRunner.RunInScope(scope =>
            {
                var instanceContext = scope.Resolve<InstanceContext>();
                var acumaticaContext = scope.Resolve<AcumaticaHttpContext>();
                var orderSync = scope.Resolve<AcumaticaOrderPut>();

                var TestInstanceId = SystemUtilities.SolicitInstanceId();
                instanceContext.Initialize(TestInstanceId);
                acumaticaContext.SessionRun(() => orderSync.RunOrder(shopifyOrderId));
            });
        }

        public static void RunAcumaticaCustomerGet()
        {
            AutofacRunner.RunInScope(scope =>
            {
                var instanceContext = scope.Resolve<InstanceContext>();
                var acumaticaContext = scope.Resolve<AcumaticaHttpContext>();
                var acumaticaCustomerGet = scope.Resolve<AcumaticaCustomerGet>();

                var TestInstanceId = SystemUtilities.SolicitInstanceId();
                instanceContext.Initialize(TestInstanceId);
                acumaticaContext.SessionRun(() =>
                {
                    acumaticaCustomerGet.RunAutomatic();
                });
            });
        }

        public static void RunAcumaticaOrderGet()
        {
            AutofacRunner.RunInScope(scope =>
            {
                var instanceContext = scope.Resolve<InstanceContext>();
                var acumaticaOrderGet = scope.Resolve<AcumaticaOrderGet>();
                var acumaticaContext = scope.Resolve<AcumaticaHttpContext>();

                var TestInstanceId = SystemUtilities.SolicitInstanceId();
                instanceContext.Initialize(TestInstanceId);
                acumaticaContext.SessionRun(() =>
                {
                    acumaticaOrderGet.RunAutomatic();
                });
            });
        }

        public static void RunAcumaticaSalesOrderRetrieve()
        {
            var defaultSalesOrderNbr = "000046";

            Console.WriteLine(Environment.NewLine 
                    + $"Enter Acumatica Sales Order ID (Default ID: {defaultSalesOrderNbr})");
            var orderNbr = Console.ReadLine().Trim().IsNullOrEmptyAlt(defaultSalesOrderNbr);

            AutofacRunner.RunInScope(scope =>
            {
                var instanceContext = scope.Resolve<InstanceContext>();
                var salesOrderClient = scope.Resolve<SalesOrderClient>();
                var acumaticaContext = scope.Resolve <AcumaticaHttpContext>();

                var TestInstanceId = SystemUtilities.SolicitInstanceId();
                instanceContext.Initialize(TestInstanceId);
                acumaticaContext.SessionRun(() =>
                {
                    var json = salesOrderClient
                        .RetrieveSalesOrder(orderNbr, SalesOrderType.SO, Expand.Details_Totals);

                    var salesOrderObj = json.ToSalesOrderObj();
                    var taxSnapshot = salesOrderObj.custom.Document.UsrTaxSnapshot;
                    var taxJson = taxSnapshot.value.Unzip();

                    Console.WriteLine(taxJson);
                });
            });
        }

        public static void RunAcumaticaPaymentGet()
        {
            AutofacRunner.RunInScope(scope =>
            {
                var instanceContext = scope.Resolve<InstanceContext>();
                var acumaticaContext = scope.Resolve<AcumaticaHttpContext>();
                var paymentClient = scope.Resolve<PaymentClient>();

                var TestInstanceId = SystemUtilities.SolicitInstanceId();
                instanceContext.Initialize(TestInstanceId);
                acumaticaContext.SessionRun(() =>
                {
                    var result = paymentClient.RetrievePayment("000014", "PMT"); // "ApplicationHistory"); 
                    
                    // "DocumentsToApply");
                });
            });
        }


        public static void RunShopifyOrderGet()
        {
            AutofacRunner.RunInScope(scope =>
            {
                var instanceContext = scope.Resolve<InstanceContext>();
                var shopifyOrderGet = scope.Resolve<ShopifyOrderGet>();

                var TestInstanceId = SystemUtilities.SolicitInstanceId();
                instanceContext.Initialize(TestInstanceId);
                shopifyOrderGet.RunAutomatic();
            });
        }


        public static void RunShopifyOrderRetrieve()
        {
            AutofacRunner.RunInScope(scope =>
            {
                var id = SolicitShopifyOrderId();

                var instanceContext = scope.Resolve<InstanceContext>();
                var shopifyOrderApi = scope.Resolve<OrderApi>();
                var logger = scope.Resolve<IPushLogger>();

                var TestInstanceId = SystemUtilities.SolicitInstanceId();
                instanceContext.Initialize(TestInstanceId);
                var json = shopifyOrderApi.Retrieve(id);
                var compressed = json.SerializeToJson(Formatting.None).ToBase64Zip();

                logger.Info($"Shopify Order size: {json.Length} bytes - (compressed {compressed.Length} bytes)");

            });
        }


        public static void RunShopifyOrderTimezoneTest()
        {
            Console.WriteLine(
                Environment.NewLine + "Enter Shopify Order ID (Default ID: 1778846826540)");

            var shopifyOrderId = Console.ReadLine().IsNullOrEmptyAlt("1778846826540").ToLong();

            AutofacRunner.RunInScope(scope =>
            {
                var instanceContext = scope.Resolve<InstanceContext>();

                var TestInstanceId = SystemUtilities.SolicitInstanceId();
                instanceContext.Initialize(TestInstanceId);

                var shopifyOrderGet = scope.Resolve<ShopifyOrderGet>();
                var acumaticaTimeZone = scope.Resolve<AcumaticaTimeZoneService>();

                var order = shopifyOrderGet.Run(shopifyOrderId).order;

                var createdAt = order.created_at;
                var createdAtUtc = order.created_at.ToUniversalTime();

                var acumaticaTime = acumaticaTimeZone.ToAcumaticaTimeZone(createdAtUtc.DateTime);
            });
        }

        public static void RunShopifyFulfillmentPut()
        {
            AutofacRunner.RunInScope(scope =>
            {
                var instanceContext = scope.Resolve<InstanceContext>();
                var shopifyFulfillmentPut = scope.Resolve<ShopifyFulfillmentPut>();

                var TestInstanceId = SystemUtilities.SolicitInstanceId();
                instanceContext.Initialize(TestInstanceId);
                shopifyFulfillmentPut.Run();
            });
        }

        public static void RunShopifyFulfillmentEmail()
        {
            var orderId = SolicitShopifyOrderId();
            var fulfillmentId = SolicitShopifyFulfillmentId(1767491698732);

            AutofacRunner.RunInScope(scope =>
            {
                var instanceContext = scope.Resolve<InstanceContext>();
                var fulfillmentApi = scope.Resolve<FulfillmentApi>();

                var json = new
                {
                    fulfillment = new
                    {
                        id = fulfillmentId,
                        notify_customer = true,
                    },
                    tracking_numbers = new []
                    {
                        "1ZY1234567890"
                    }
                }.SerializeToJson();

                var TestInstanceId = SystemUtilities.SolicitInstanceId();
                instanceContext.Initialize(TestInstanceId);
                fulfillmentApi.Update(orderId, fulfillmentId, json);
            });
        }

        public static void RunShopifyDataFeeder()
        {
            AutofacRunner.RunInScope(scope =>
            {
                var feeder = scope.Resolve<ShopifyDataFeeder>();
                feeder.Run();
            });
        }
    }
}
