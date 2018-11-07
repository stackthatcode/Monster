using System;
using System.Collections.Generic;
using System.Linq;

namespace Monster.Middle.Persist.Multitenant
{
    public class PayoutRepository
    {
        private readonly PersistContext _persistContext;
        public MonsterDataContext Entities => _persistContext.Entities;

        public PayoutRepository(PersistContext dataContext)
        {
            _persistContext = dataContext;
        }

        //
        // Preferences
        //
        public UsrPayoutPreference RetrievePayoutPreferences()
        {
            return Entities.UsrPayoutPreferences.FirstOrDefault();
        }


        //
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

        public List<UsrShopifyPayout> RetrieveIncompletePayoutImports()
        {
            return Entities
                    .UsrShopifyPayouts
                    .Where(x => x.AllShopifyTransDownloaded == false)
                    .OrderBy(x => x.ShopifyPayoutId)
                    .ToList();
        }

        public List<UsrShopifyPayout> RetrievePayouts()
        {
            return Entities
                    .UsrShopifyPayouts
                    .OrderBy(x => x.ShopifyPayoutId)
                    .ToList();
        }

        public List<UsrShopifyPayout> RetrieveNotYetUploadedPayouts()
        {
            return Entities
                    .UsrShopifyPayouts
                    .Where(x => x.UsrShopifyPayoutTransactions.Any(y => y.AcumaticaImportDate == null)
                                && x.ShopifyLastStatus != "in_transit")
                    .OrderBy(x => x.ShopifyPayoutId)
                    .ToList();
        }

        public void InsertPayoutHeader(UsrShopifyPayout payout)
        {
            Entities.UsrShopifyPayouts.Add(payout);
            Entities.SaveChanges();
        }

        public void UpdatePayoutHeaderStatus(
                        long shopifyPayoutId, string status)
        {
            var payout = RetrievePayout(shopifyPayoutId);
            payout.ShopifyLastStatus = status;
            Entities.SaveChanges();
        }

        public void UpdatePayoutHeaderAllShopifyTransDownloaded(
                        long shopifyPayoutId, bool captured)
        {
            var header = RetrievePayout(shopifyPayoutId);
            header.AllShopifyTransDownloaded = captured;
            Entities.SaveChanges();
        }

        public void UpdatePayoutHeaderAcumaticaImport(
                long shopifyPayoutId, 
                string acumaticaCashAccount,
                string acumaticaRefNumber, 
                DateTime acumaticaImportDate)
        {
            var header = RetrievePayout(shopifyPayoutId);
            header.AcumaticaCashAccount = acumaticaCashAccount;
            header.AcumaticaRefNumber = acumaticaRefNumber;
            header.AcumaticaImportDate = acumaticaImportDate;
            Entities.SaveChanges();
        }



        //
        // Transactions aka Payout Detail
        //
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

        public List<UsrShopifyPayoutTransaction>
                    RetrieveNotYetUploadedPayoutTranscations(long shopifyPayoutId)
        {
            return Entities
                .UsrShopifyPayoutTransactions
                .Where(x => 
                        x.ShopifyPayoutId == shopifyPayoutId && 
                        x.AcumaticaImportDate == null)
                .ToList();
        }

        public void InsertPayoutTransaction(
                        UsrShopifyPayoutTransaction transaction)
        {
            Entities.UsrShopifyPayoutTransactions.Add(transaction);
            Entities.SaveChanges();
        }

        public void UpdatePayoutHeaderAcumaticaImport(
                        long shopifyPayoutId,
                        long shopifyPayoutTransId,
                        DateTime acumaticaImportDate,
                        string acumaticaExtRefNbr)
        {
            var transaction = RetrievePayoutTransaction(shopifyPayoutId, shopifyPayoutTransId);
            transaction.AcumaticaImportDate = acumaticaImportDate;
            transaction.AcumaticaExtRefNbr = acumaticaExtRefNbr;
            Entities.SaveChanges();
        }

    }
}

