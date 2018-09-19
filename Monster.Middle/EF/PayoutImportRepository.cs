using System;
using System.Collections.Generic;
using System.Linq;

namespace Monster.Middle.EF
{
    public class PayoutImportRepository
    {
        private readonly MonsterDataContext _dataContext;

        public PayoutImportRepository(MonsterDataContext dataContext)
        {
            _dataContext = dataContext;
        }

        //
        // Preferences
        //
        public UsrPayoutPreference RetrievePayoutPreferences()
        {
            return _dataContext.UsrPayoutPreferences.FirstOrDefault();
        }


        //
        // Payouts aka Payout Headers
        //
        public List<UsrShopifyPayout> RetrievePayouts(int limit = 50)
        {
            return _dataContext
                    .UsrShopifyPayouts
                    .OrderByDescending(x => x.ShopifyPayoutId)
                    .Take(limit)
                    .ToList();
        }

        public UsrShopifyPayout RetrievePayout(long shopifyPayoutId)
        {
            return _dataContext
                    .UsrShopifyPayouts
                    .FirstOrDefault(x => x.ShopifyPayoutId == shopifyPayoutId);
        }

        public List<UsrShopifyPayout> RetrieveIncompletePayoutImports()
        {
            return _dataContext
                    .UsrShopifyPayouts
                    .Where(x => x.AllShopifyTransDownloaded == false)
                    .OrderBy(x => x.ShopifyPayoutId)
                    .ToList();
        }

        public List<UsrShopifyPayout> RetrievePayouts()
        {
            return _dataContext
                .UsrShopifyPayouts
                .OrderBy(x => x.ShopifyPayoutId)
                .ToList();
        }

        public List<UsrShopifyPayout> RetrieveNotYetUploadedPayouts()
        {
            return _dataContext
                .UsrShopifyPayouts
                .Where(x => x.AcumaticaRefNumber == null 
                            && x.ShopifyLastStatus != "in_transit")
                .OrderBy(x => x.ShopifyPayoutId)
                .ToList();
        }

        public int InsertPayoutHeader(UsrShopifyPayout payout)
        {
            _dataContext.UsrShopifyPayouts.Add(payout);
            _dataContext.SaveChanges();
            return payout.Id;
        }

        public void UpdatePayoutHeaderStatus(
                        long shopifyPayoutId, string status)
        {
            var payout = RetrievePayout(shopifyPayoutId);
            payout.ShopifyLastStatus = status;
            _dataContext.SaveChanges();
        }

        public void UpdatePayoutHeaderAllShopifyTransDownloaded(
                        long shopifyPayoutId, bool captured)
        {
            var header = RetrievePayout(shopifyPayoutId);
            header.AllShopifyTransDownloaded = captured;
            _dataContext.SaveChanges();
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
            _dataContext.SaveChanges();
        }



        //
        // Transactions aka Payout Detail
        //
        public UsrShopifyPayoutTransaction
                    RetrievePayoutTransaction(
                        long shopifyPayoutId, long shopifyTransactionId)
        {
            return _dataContext
                .UsrShopifyPayoutTransactions
                .FirstOrDefault(
                    x => x.ShopifyPayoutId == shopifyPayoutId &&
                         x.ShopifyPayoutTransId == shopifyTransactionId);
        }

        public List<UsrShopifyPayoutTransaction>
                    RetrieveNotYetUploadedPayoutTranscations(long shopifyPayoutId)
        {
            return _dataContext
                .UsrShopifyPayoutTransactions
                .Where(x => 
                        x.ShopifyPayoutId == shopifyPayoutId && 
                        x.AcumaticaImportDate == null)
                .ToList();
        }

        public int InsertPayoutTransaction(
                        UsrShopifyPayoutTransaction transaction)
        {
            _dataContext.UsrShopifyPayoutTransactions.Add(transaction);
            _dataContext.SaveChanges();
            return transaction.Id;
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
            _dataContext.SaveChanges();
        }

    }
}

