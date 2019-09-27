using System;
using System.Collections.Generic;
using System.Linq;
using Monster.Middle.Persist.Instance;
using Monster.Middle.Persist.Instance;

namespace Monster.Middle.Processes.Shopify.Persist
{
    public class ShopifyPayoutRepository
    {
        private readonly InstancePersistContext _persistContext;
        public MonsterDataContext Entities => _persistContext.Entities;

        public ShopifyPayoutRepository(InstancePersistContext dataContext)
        {
            _persistContext = dataContext;
        }


        // Preferences
        //
        public UsrPayoutPreference RetrievePayoutPreferences()
        {
            return Entities.UsrPayoutPreferences.FirstOrDefault();
        }


        // Payouts aka Payout Headers
        //
        public List<UsrShopifyPayout> RetrievePayouts(int limit = 50)
        {
            return Entities
                    .UsrShopifyPayouts
                    .OrderByDescending(x => x.ShopifyPayoutId)
                    .Take(limit)
                    .ToList();
        }

        public UsrShopifyPayout RetrievePayout(long shopifyPayoutId)
        {
            return Entities
                    .UsrShopifyPayouts
                    .FirstOrDefault(x => x.ShopifyPayoutId == shopifyPayoutId);
        }

        public List<UsrShopifyPayout> RetrievePayoutsNotYetProcessedByBank()
        {
            return Entities
                    .UsrShopifyPayouts
                    .Where(x => x.ShopifyLastStatus == "scheduled" ||
                                x.ShopifyLastStatus == "in_transit")
                    .ToList();
        }

        public List<UsrShopifyPayout> RetrieveIncompletePayoutImports()
        {
            return Entities
                    .UsrShopifyPayouts
                    .Where(x => x.AllTransDownloaded == false)
                    .OrderBy(x => x.ShopifyPayoutId)
                    .ToList();
        }

        
        public void InsertPayoutHeader(UsrShopifyPayout payout)
        {
            Entities.UsrShopifyPayouts.Add(payout);
            Entities.SaveChanges();
        }

        public void UpdatePayoutHeader(
                long shopifyPayoutId, string json, string status)
        {
            var payout = RetrievePayout(shopifyPayoutId);
            payout.Json = json;
            payout.ShopifyLastStatus = status;
            payout.LastUpdated = DateTime.UtcNow;
            Entities.SaveChanges();
        }

        public void UpdatePayoutHeaderAllTransDownloaded(
                        long shopifyPayoutId, bool captured)
        {
            var header = RetrievePayout(shopifyPayoutId);
            header.AllTransDownloaded = captured;
            header.LastUpdated = DateTime.UtcNow;
            Entities.SaveChanges();
        }


        // Transactions aka Payout Detail
        //

        public List<UsrShopifyPayoutTransaction> 
                RetrievePayoutTransactions(long shopifyPayoutId)
        {
            return Entities.UsrShopifyPayoutTransactions
                    .Where(x => x.ShopifyPayoutId == shopifyPayoutId)
                    .ToList();
        }

        public UsrShopifyPayoutTransaction
                    RetrievePayoutTransaction(
                        long shopifyPayoutId, long shopifyTransactionId)
        {
            return Entities
                    .UsrShopifyPayoutTransactions
                    .FirstOrDefault(
                        x => x.ShopifyPayoutId == shopifyPayoutId &&
                             x.ShopifyPayoutTransId == shopifyTransactionId);
        }
        
        public void InsertPayoutTransaction(
                        UsrShopifyPayoutTransaction transaction)
        {
            Entities.UsrShopifyPayoutTransactions.Add(transaction);
            Entities.SaveChanges();
        }
    }
}

