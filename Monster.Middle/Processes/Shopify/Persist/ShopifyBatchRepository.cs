using System;
using System.Linq;
using Monster.Middle.Persist.Tenant;

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
        
        public void UpdateProductsPullEnd(DateTime endTimeUtc)
        {
            var existingState = Retrieve();
            existingState.ShopifyProductsPullEnd = endTimeUtc;
            Entities.SaveChanges();
        }

        public void UpdateOrdersPullEnd(DateTime endTimeUtc)
        {
            var existingState = Retrieve();
            existingState.ShopifyOrdersPullEnd = endTimeUtc;
            Entities.SaveChanges();
        }

        public void UpdateCustomersPullEnd(DateTime endTimeUtc)
        {
            var existingState = Retrieve();
            existingState.ShopifyCustomersPullEnd = endTimeUtc;
            Entities.SaveChanges();
        }
        

        private static readonly object _lock = new object();

        public UsrShopifyBatchState Retrieve()
        {
            lock (_lock)
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
        }

        public void SaveChanges()
        {
            _dataContext.Entities.SaveChanges();
        }
    }
}

