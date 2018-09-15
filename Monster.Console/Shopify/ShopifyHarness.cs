using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using Push.Foundation.Utilities.Json;
using Push.Foundation.Utilities.Logging;
using Push.Shopify.Api;
using Push.Shopify.Api.Order;
using Push.Shopify.Api.Payout;
using Push.Shopify.Api.Product;
using Push.Shopify.Api.Transaction;
using Push.Shopify.Config;
using Push.Shopify.Http.Credentials;
using Transaction = Push.Shopify.Api.Payout.Transaction;

namespace Monster.ConsoleApp.Shopify
{
    public class ShopifyHarness
    {
        public static IShopifyCredentials CredentialsFactory()
        {
            return ShopifySecuritySettings
                .FromConfiguration()
                .MakePrivateAppCredentials();
        }

        public static void RetrieveOrderData(ILifetimeScope scope, long orderId)
        {
            var factory = scope.Resolve<ShopifyApiFactory>();
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

        public static void RetrieveProductData(ILifetimeScope scope, long productId)
        {
            var factory = scope.Resolve<ShopifyApiFactory>();
            var credentials = CredentialsFactory();

            var productApi = factory.MakeProductApi(credentials);
            var shopifyOrderJson = productApi.Retrieve(productId);

            var productParent = shopifyOrderJson.DeserializeFromJson<ProductParent>();
            productParent.Initialize();
            var monsterProductJson = productParent.SerializeToJson();

            var inventoryItemIDs = productParent.product.InventoryItemIds;
            var shopifyInventoryLevels = productApi.RetrieveInventoryLevels(inventoryItemIDs);
        }

        public static void RetrieveLocations(ILifetimeScope scope)
        {
            var factory = scope.Resolve<ShopifyApiFactory>();
            var credentials = CredentialsFactory();

            var productApi = factory.MakeProductApi(credentials);
            var shopifyLocationJson = productApi.RetrieveLocations();

            var locations =
                shopifyLocationJson
                    .DeserializeFromJson<LocationList>();

            var monsterLocationJson = locations.SerializeToJson();
        }
        
    }
}
