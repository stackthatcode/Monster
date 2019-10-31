using System;
using System.Collections.Generic;
using System.Linq;
using Monster.Middle.Persist.Instance;
using Monster.Middle.Persist.Instance;

namespace Monster.Middle.Processes.Shopify.Persist
{
    public class ShopifyPayoutRepository
    {
        private readonly ProcessPersistContext _persistContext;
        public MonsterDataContext Entities => _persistContext.Entities;

        public ShopifyPayoutRepository(ProcessPersistContext dataContext)
        {
            _persistContext = dataContext;
        }


        // Settingss
        //
        public PayoutSettings RetrievePayoutSettingss()
        {
            return Entities.PayoutSettingss.FirstOrDefault();
        }


        // Payouts aka Payout Headers
        //
        public List<ShopifyPayout> RetrievePayouts(int limit = 50)
        {
            return Entities
                    .ShopifyPayouts
                    .OrderByDescending(x => x.ShopifyPayoutId)
                    .Take(limit)
                    .ToList();
        }

        public ShopifyPayout RetrievePayout(long shopifyPayoutId)
        {
            return Entities
                    .ShopifyPayouts
                    .FirstOrDefault(x => x.ShopifyPayoutId == shopifyPayoutId);
        }

        public List<ShopifyPayout> RetrievePayoutsNotYetProcessedByBank()
        {
            return Entities
                    .ShopifyPayouts
                    .Where(x => x.ShopifyLastStatus == "scheduled" ||
                                x.ShopifyLastStatus == "in_transit")
                    .ToList();
        }

        public List<ShopifyPayout> RetrieveIncompletePayoutImports()
        {
            return Entities
                    .ShopifyPayouts
                    .Where(x => x.AllTransDownloaded == false)
                    .OrderBy(x => x.ShopifyPayoutId)
                    .ToList();
        }

        
        public void InsertPayoutHeader(ShopifyPayout payout)
        {
            Entities.ShopifyPayouts.Add(payout);
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

        public List<ShopifyPayoutTransaction> 
                RetrievePayoutTransactions(long shopifyPayoutId)
        {
            return Entities.ShopifyPayoutTransactions
                    .Where(x => x.ShopifyPayoutId == shopifyPayoutId)
                    .ToList();
        }

        public ShopifyPayoutTransaction
                    RetrievePayoutTransaction(
                        long shopifyPayoutId, long shopifyTransactionId)
        {
            return Entities
                    .ShopifyPayoutTransactions
                    .FirstOrDefault(
                        x => x.ShopifyPayoutId == shopifyPayoutId &&
                             x.ShopifyPayoutTransId == shopifyTransactionId);
        }
        
        public void InsertPayoutTransaction(
                        ShopifyPayoutTransaction transaction)
        {
            Entities.ShopifyPayoutTransactions.Add(transaction);
            Entities.SaveChanges();
        }
    }
}

