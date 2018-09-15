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
                    .Where(x => x.AllDetailRecordsCaptured == false)
                    .OrderBy(x => x.ShopifyPayoutId)
                    .ToList();
        }

        public int InsertPayoutHeader(UsrShopifyPayout payout)
        {
            _dataContext.UsrShopifyPayouts.Add(payout);
            _dataContext.SaveChanges();
            return payout.Id;
        }

        public void UpdatePayoutHeaderAllRecordsCaptured(
                        long shopifyPayoutId, bool captured)
        {
            var header = RetrievePayout(shopifyPayoutId);
            header.AllDetailRecordsCaptured = captured;
            _dataContext.SaveChanges();
        }


        public UsrShopifyPayoutTransaction
                    RetrievePayoutTransaction(long shopifyPayoutId, long shopifyTransactionId)
        {
            return _dataContext
                .UsrShopifyPayoutTransactions
                .FirstOrDefault(
                    x => x.ShopifyPayoutId == shopifyPayoutId &&
                         x.ShopifyPayoutTransId == shopifyTransactionId);
        }

        public List<UsrShopifyPayoutTransaction>
                    RetrievePayoutTranscations(long shopifyPayoutId)
        {
            return _dataContext
                .UsrShopifyPayoutTransactions
                .Where(x => x.ShopifyPayoutId == shopifyPayoutId)
                .ToList();
        }

        public int InsertPayoutTransaction(
                        UsrShopifyPayoutTransaction transaction)
        {
            _dataContext.UsrShopifyPayoutTransactions.Add(transaction);
            _dataContext.SaveChanges();
            return transaction.Id;
        }
    }
}

