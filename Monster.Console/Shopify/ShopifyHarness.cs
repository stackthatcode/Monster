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
using Push.Shopify.HttpClient.Credentials;
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

        public static void RetrieveProductData(ILifetimeScope scope, long productId)
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

        public static void RetrieveLocations(ILifetimeScope scope)
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

        public static void RetrievePayoutData(ILifetimeScope scope)
        {
            var factory = scope.Resolve<ApiFactory>();
            var credentials = CredentialsFactory();
            var payoutApi = factory.MakePayoutApi(credentials);
            var logger = scope.Resolve<IPushLogger>();
 

            // Get current list of Payouts
            var payoutJson = payoutApi.RetrievePayouts();
            logger.Info("Raw Json from payout: " + payoutJson);
            var payouts = payoutJson.DeserializeFromJson<PayoutList>();

            // Grab the most recent Payout Id
            var payoutId = payouts.payouts.First().id;

            var transactions = new List<Transaction>();
            var limit = 5;

            // Read first batch
            var firstBatchJson =
                payoutApi.RetrievePayoutDetail(payout_id: payoutId, limit: limit);

            logger.Info("Raw Json from detail: " + firstBatchJson);

            var firstBatchTransactions
                = firstBatchJson.DeserializeFromJson<PayoutDetail>().transactions;

            transactions.AddRange(firstBatchTransactions);
            long lastTranscationId = firstBatchTransactions.Last().id;

            while (true)
            {
                // Grab the next Batch
                var nextBatchJson = 
                    payoutApi.RetrievePayoutDetail(
                        since_id: lastTranscationId, limit: limit);

                logger.Info("Raw Json from detail: " + nextBatchJson);

                var nextBatch = nextBatchJson.DeserializeFromJson<PayoutDetail>();
                transactions.AddRange(nextBatch.transactions);

                // We stop iterating when we see type = "payout"
                if (transactions.Any(x => x.type == "payout"))
                {
                    break;
                }

                // Else, grab the last transaction id and keep going!
                lastTranscationId = nextBatch.transactions.Last().id;
            }

            // We'll dump our output to JSON for easy review
            logger.Info("Unsanitized output (all transactions):");
            logger.Info(transactions.SerializeToJson());

            var filteredTransactions 
                = transactions.Where(x => x.payout_id != null).ToList();

            logger.Info("Much better!:");
            logger.Info(filteredTransactions.SerializeToJson());

            logger.Info(Environment.NewLine + "Ok, now time to balance!" + Environment.NewLine);

            // Total up all of the charges
            var charges = filteredTransactions.Where(x => x.type == "charge").ToList();
            logger.Info($"Total Charges -> Amount: {charges.Sum(x => x.amount)}");
            logger.Info($"Total Charges -> Fee: {charges.Sum(x => x.fee)}");
            logger.Info($"Total Charges -> Net: {charges.Sum(x => x.net)}");

            var payout = filteredTransactions.First(x => x.type == "payout");
            logger.Info($"Payout -> Amount: {payout.amount}");
            logger.Info($"Payout -> Fee: {payout.fee}");
            logger.Info($"Payout -> Net: {payout.net}");            
        }

    }
}
