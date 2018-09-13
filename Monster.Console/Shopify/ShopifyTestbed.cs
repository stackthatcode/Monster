using System.Linq;
using Autofac;
using Push.Foundation.Utilities.Json;
using Push.Shopify.Api;
using Push.Shopify.Api.Order;
using Push.Shopify.Api.Payout;
using Push.Shopify.Api.Product;
using Push.Shopify.Api.Transaction;
using Push.Shopify.Config;
using Push.Shopify.HttpClient.Credentials;

namespace Monster.ConsoleApp.Shopify
{
    public class ShopifyTestbed
    {
        public static IShopifyCredentials CredentialsFactory()
        {
            return ShopifySecuritySettings
                    .FromConfiguration()
                    .MakePrivateAppCredentials();
        }

        public void RetrieveOrderData(ILifetimeScope scope, long orderId)
        {
            var factory = scope.Resolve<ApiFactory>();
            var credentials = CredentialsFactory();

            var orderApi = factory.MakeOrderApi(credentials);
            var shopifyOrderJson = orderApi.Retrieve(orderId);

            var orderParent = shopifyOrderJson.DeserializeFromJson<OrderParent>();
            orderParent.order.Initialize();
            var monsterOrderJson = orderParent.SerializeToJson();

            var shopifyTransactionJson = orderApi.RetrieveTransactions(orderId);
            var transactionParent =
                    shopifyTransactionJson.DeserializeFromJson<TransactionList>();

            var monsterTransactionJson = transactionParent.SerializeToJson();
        }

        public void RetrieveProductData(ILifetimeScope scope, long productId)
        {
            var factory = scope.Resolve<ApiFactory>();
            var credentials = CredentialsFactory();

            var productApi = factory.MakeProductApi(credentials);
            var shopifyOrderJson = productApi.Retrieve(productId);

            var productParent = shopifyOrderJson.DeserializeFromJson<ProductParent>();
            productParent.Initialize();
            var monsterProductJson = productParent.SerializeToJson();

            var inventoryItemIDs = productParent.product.InventoryItemIds;
            var shopifyInventoryLevels = productApi.RetrieveInventoryLevels(inventoryItemIDs);
        }

        public void RetrieveLocations(ILifetimeScope scope)
        {
            var factory = scope.Resolve<ApiFactory>();
            var credentials = CredentialsFactory();

            var productApi = factory.MakeProductApi(credentials);
            var shopifyLocationJson = productApi.RetrieveLocations();

            var locations =
                shopifyLocationJson
                    .DeserializeFromJson<LocationList>();

            var monsterLocationJson = locations.SerializeToJson();
        }

        public void RetrievePayoutDta(ILifetimeScope scope)
        {
            var factory = scope.Resolve<ApiFactory>();
            var credentials = CredentialsFactory();
            var payoutApi = factory.MakePayoutApi(credentials);

            //var date = new DateTimeOffset(2018, 8, 8, 0, 0, 0, new TimeSpan(0, 0, 0));

            var shopifyPayoutJson = payoutApi.RetrievePayouts();
            var monsterPayout = shopifyPayoutJson.DeserializeFromJson<PayoutList>();
            var firstPayoutId = monsterPayout.payouts.First().id;

            var shopifyPayoutDetailJson = payoutApi.RetrievePayoutDetail(firstPayoutId);
            var monsterPayoutDetail = shopifyPayoutDetailJson.DeserializeFromJson<PayoutDetail>();
        }

    }
}
