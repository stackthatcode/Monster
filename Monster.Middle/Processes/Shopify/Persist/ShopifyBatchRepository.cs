using System;
using System.Linq;
using Monster.Middle.Persist.Multitenant;

namespace Monster.Middle.Processes.Shopify.Persist
{
    public class ShopifyBatchRepository
    {
        private readonly PersistContext _dataContext;
        public MonsterDataContext Entities => _dataContext.Entities;
        
        public ShopifyBatchRepository(PersistContext dataContext)
        {
            _dataContext = dataContext;
        }
        

        public void Reset()
        {
            var existingState 
                = Entities.UsrShopifyBatchStates.FirstOrDefault();

            if (existingState != null)
            {
                Entities.UsrShopifyBatchStates.Remove(existingState);
            }

            var newState = new UsrShopifyBatchState();
            Entities.UsrShopifyBatchStates.Add(newState);
            Entities.SaveChanges();
        }
        
        public void UpdateShopifyProductsPullEnd(DateTime endTimeUtc)
        {
            var existingState = Retrieve();
            existingState.ShopifyProductsPullEnd = endTimeUtc;
            Entities.SaveChanges();
        }

        public void UpdateShopifyOrdersPullEnd(DateTime endTimeUtc)
        {
            var existingState = Retrieve();
            existingState.ShopifyOrdersPullEnd = endTimeUtc;
            Entities.SaveChanges();
        }

        public void UpdateShopifyCustomersPullEnd(DateTime endTimeUtc)
        {
            var existingState = Retrieve();
            existingState.ShopifyCustomersPullEnd = endTimeUtc;
            Entities.SaveChanges();
        }
        

        public UsrShopifyBatchState Retrieve()
        {
            var output = 
                _dataContext
                    .Entities
                    .UsrShopifyBatchStates.FirstOrDefault();

            if (output == null)
            {
                var newState = new UsrShopifyBatchState();
                Entities.UsrShopifyBatchStates.Add(newState);
                return newState;
            }
            else
            {
                return output;
            }
        }

        public void SaveChanges()
        {
            _dataContext.Entities.SaveChanges();
        }
    }
}

