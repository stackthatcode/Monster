using System;
using System.Linq;

namespace Monster.Middle.Persist.Multitenant
{
    public class BatchStateRepository
    {
        private readonly PersistContext _dataContext;
        public MonsterDataContext Entities => _dataContext.Entities;
        
        public BatchStateRepository(PersistContext dataContext)
        {
            _dataContext = dataContext;
        }
        

        public void ResetShopifyBatchState()
        {
            var existingState = Entities.UsrBatchStates.FirstOrDefault();

            if (existingState == null)
            {
                var newState = new UsrBatchState()
                {
                    ShopifyProductsEndDate = null,
                    ShopifyOrdersStartDate = null,
                    ShopifyOrdersEndDate = null,
                };

                Entities.UsrBatchStates.Add(newState);
            }
            else
            {
                existingState.ShopifyProductsEndDate = null;
                existingState.ShopifyOrdersStartDate = null;
                existingState.ShopifyOrdersEndDate = null;
            }

            Entities.SaveChanges();
        }

        public void UpdateShopifyProductsEnd(DateTime endTimeUtc)
        {
            var existingState = Entities.UsrBatchStates.First();
            existingState.ShopifyProductsEndDate = endTimeUtc;
            Entities.SaveChanges();
        }

        public void UpdateForShopifyOrdersStart()
        {

        }
    }
}
