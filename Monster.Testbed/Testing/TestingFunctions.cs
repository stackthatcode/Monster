using System;

namespace Monster.Testbed.Testing
{
    class TestingFunctions
    {

        // Testing functions
        //
        private const string ShopifyOrderTimezoneTest = "101";
        private const string ShopifyOrderGet = "102";
        private const string AcumaticaCustomerGet = "103";
        private const string AcumaticaOrderGet = "104";
        private const string AcumaticaOrderSync = "105";
        private const string AcumaticaPaymentGet = "106";
        private const string AcumaticaSalesOrderRetrieve = "107";
        private const string ShopifyFulfillmentPut = "108";
        private const string ShopifyFulfillmentEmail = "109";
        private const string ShopifyRetrieveOrder = "110";
        private const string ShopifyDataFeeder = "200";


        private static void DisplayTestingFunctions()
        {
            // Component testing functions
            //
            Console.WriteLine();
            Console.WriteLine($"{ShopifyOrderTimezoneTest} - Shopify Order to Acumatica Timezone Test");
            Console.WriteLine($"{ShopifyOrderGet} - Shopify Order Get (Automatic)");
            Console.WriteLine($"{AcumaticaCustomerGet} - Acumatica Customer Get");
            Console.WriteLine($"{AcumaticaOrderGet} - Acumatica Order Get");
            Console.WriteLine($"{AcumaticaOrderSync} - Acumatica Order Sync (Order ID)");
            Console.WriteLine($"{AcumaticaPaymentGet} - Acumatica Payment Get");
            Console.WriteLine($"{AcumaticaSalesOrderRetrieve} - Acumatica Sales Order Retrieve");
            Console.WriteLine($"{ShopifyFulfillmentPut} - Shopify Fulfillment Put");
            Console.WriteLine($"{ShopifyFulfillmentEmail} - Shopify Fulfillment Email");
            Console.WriteLine($"{ShopifyRetrieveOrder} - Measure Shopify Order JSON storage size");
            Console.WriteLine();
            Console.WriteLine($"{ShopifyDataFeeder} - Run Shopify Data feeder");
            Console.WriteLine();

            Console.WriteLine(Environment.NewLine + "Make a selection and hit ENTER:");
            var input = Console.ReadLine();

            // Testing functions
            //
            if (input == ShopifyOrderTimezoneTest)
                RandomStuff.RunShopifyOrderTimezoneTest();
            if (input == ShopifyOrderGet)
                RandomStuff.RunShopifyOrderGet();
            if (input == AcumaticaOrderSync)
                RandomStuff.RunAcumaticaOrderSync();
            if (input == AcumaticaCustomerGet)
                RandomStuff.RunAcumaticaCustomerGet();
            if (input == AcumaticaOrderGet)
                RandomStuff.RunAcumaticaOrderGet();
            if (input == AcumaticaPaymentGet)
                RandomStuff.RunAcumaticaPaymentGet();
            if (input == AcumaticaSalesOrderRetrieve)
                RandomStuff.RunAcumaticaSalesOrderRetrieve();
            if (input == ShopifyFulfillmentPut)
                RandomStuff.RunShopifyFulfillmentPut();
            if (input == ShopifyFulfillmentEmail)
                RandomStuff.RunShopifyFulfillmentEmail();
            if (input == ShopifyRetrieveOrder)
                RandomStuff.RunShopifyOrderRetrieve();

            //if (input == ShopifyDataFeeder)
            //    MoreStuff.RunShopifyDataFeeder();
        }
    }
}
