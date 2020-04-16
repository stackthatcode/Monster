using System;
using Push.Foundation.Utilities.Helpers;


namespace Monster.Testbed
{
    public class CommandLineHelpers
    {
        private static string DefaultInstanceId = "51AA413D-E679-4F38-BA47-68129B3F9212";
        private static string DefaultSalesOrderNbr = "000046";

        public static Guid SolicitInstanceId()
        {
            Console.WriteLine(Environment.NewLine + $"Enter Instance Id (Default Id: {DefaultInstanceId})");
            return Guid.Parse(Console.ReadLine().IsNullOrEmptyAlt(DefaultInstanceId));
        }

        public static long SolicitShopifyFulfillmentId(long defaultId)
        {
            Console.WriteLine(Environment.NewLine + $"Enter Shopify Fulfillment Id (Default Id: {defaultId})");
            return Console.ReadLine().IsNullOrEmptyAlt(defaultId.ToString()).ToLong();
        }


        public static long SolicitOrderShopifyId()
        {
            Console.WriteLine(Environment.NewLine + "Enter Shopify Order Id (Default Id: 1949921378348)");
            return NumberHelpers.ToLong(Console.ReadLine().IsNullOrEmptyAlt("1949921378348	"));
        }

        public static string SolicitAcumaticaSalesOrderId()
        {
            Console.WriteLine(Environment.NewLine
                              + $"Enter Acumatica Sales Order ID (Default ID: {DefaultSalesOrderNbr})");
            var orderNbr = Console.ReadLine().Trim().IsNullOrEmptyAlt(DefaultSalesOrderNbr);
            return orderNbr;
        }

    }
}
