using System;
using Push.Foundation.Utilities.Helpers;

namespace Monster.ConsoleApp
{
    public class CommandLineFuncs
    {
        public static bool Confirm(string confirmMsg)
        {
            Console.WriteLine(
                Environment.NewLine + confirmMsg + Environment.NewLine + "Please type 'YES' to proceed");
            var input = Console.ReadLine();
            return input.Trim() == "YES";
        }

        private static string TestInstanceId = "51AA413D-E679-4F38-BA47-68129B3F9212";
        private static string DefaultSalesOrderNbr = "000046";


        public static Guid SolicitInstanceId()
        {
            Console.WriteLine(Environment.NewLine + $"Enter Instance Id (Default Id: {TestInstanceId})");
            return Guid.Parse(Console.ReadLine().IsNullOrEmptyAlt(TestInstanceId));
        }

        public static long SolicitShopifyId()
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
