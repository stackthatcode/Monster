using System.Linq;
using Monster.Middle.Persist.Instance;
using Push.Foundation.Utilities.Json;
using Push.Shopify.Api.Order;
using Push.Shopify.Api.Transactions;


namespace Monster.Middle.Processes.Shopify.Persist
{
    public static class OrderExtensions
    {
        public static Order ToShopifyObj(this ShopifyOrder input)
        {
            return input.ShopifyJson.DeserializeToOrder();
        }

        public static Transaction ToShopifyObj(this ShopifyTransaction input)
        {
            return input.ShopifyJson.DeserializeFromJson<Transaction>();
        }

        public static bool ChangesDetected(this ShopifyOrder currentRecord, Order newOrder)
        {
            return
                currentRecord.ShopifyIsCancelled != newOrder.IsCancelled ||
                currentRecord.ShopifyFinancialStatus != newOrder.financial_status;
        }
    }
}

