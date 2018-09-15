using System.Collections.Generic;
using System.Linq;
using Push.Shopify.Api;

namespace Monster.Middle.EF
{
    public class PayoutImportRepository
    {
        private readonly MonsterDataContext _dataContext;

        public PayoutImportRepository(MonsterDataContext dataContext)
        {
            _dataContext = dataContext;
        }

        public List<UsrShopifyPayout> RetrievePayouts()
        {
            return _dataContext.UsrShopifyPayouts.ToList();
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
                    .OrderBy(x => x.CreatedDate)
                    .ToList();
        }

        public int InsertPayoutHeader(UsrShopifyPayout payout)
        {
            _dataContext.UsrShopifyPayouts.Add(payout);
            _dataContext.SaveChanges();
            return payout.Id;
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

        public int InsertPayoutTransaction(
                        UsrShopifyPayoutTransaction transaction)
        {
            _dataContext.UsrShopifyPayoutTransactions.Add(transaction);
            _dataContext.SaveChanges();
            return transaction.Id;
        }
    }
}

