using System;
using System.Linq;
using Monster.Middle.Persist.Instance;
using Monster.Middle.Persist.Instance;

namespace Monster.Middle.Processes.Shopify.Persist
{
    public class ShopifyBatchRepository
    {
        private readonly ProcessPersistContext _dataContext;
        public MonsterDataContext Entities => _dataContext.Entities;
        
        public ShopifyBatchRepository(ProcessPersistContext dataContext)
        {
            _dataContext = dataContext;
        }
        

        public void Reset()
        {
            var existingState 
                = Entities.ShopifyBatchStates.FirstOrDefault();

            if (existingState != null)
            {
                Entities.ShopifyBatchStates.Remove(existingState);
            }

            var newState = new ShopifyBatchState();
            Entities.ShopifyBatchStates.Add(newState);
            Entities.SaveChanges();
        }
        
        public void UpdateProductsGetEnd(DateTime endTimeUtc)
        {
            var existingState = Retrieve();
            existingState.ShopifyProductsGetEnd = endTimeUtc;
            Entities.SaveChanges();
        }

        public void UpdateOrdersGetEnd(DateTime endTimeUtc)
        {
            var existingState = Retrieve();
            existingState.ShopifyOrdersGetEnd = endTimeUtc;
            Entities.SaveChanges();
        }

        public void UpdateCustomersGetEnd(DateTime endTimeUtc)
        {
            var existingState = Retrieve();
            existingState.ShopifyCustomersGetEnd = endTimeUtc;
            Entities.SaveChanges();
        }
        

        private static readonly object _lock = new object();

        public ShopifyBatchState Retrieve()
        {
            lock (_lock)
            {
                var output =
                    _dataContext
                        .Entities
                        .ShopifyBatchStates.FirstOrDefault();

                if (output == null)
                {
                    var newState = new ShopifyBatchState();
                    Entities.ShopifyBatchStates.Add(newState);
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

