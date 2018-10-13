using System;
using System.Linq;

namespace Monster.Middle.Persist.Multitenant
{
    public class BatchStateRepository
    {
        private readonly PersistContext _dataContext;
        public MonsterDataContext Entities => _dataContext.Entities;

        public int FudgeFactorSeconds = 180;

        public BatchStateRepository(PersistContext dataContext)
        {
            _dataContext = dataContext;
        }

        public DateTime ComputeEndTimeStamp()
        {
            return DateTime.UtcNow.AddSeconds(-FudgeFactorSeconds);
        }

        public void ResetShopifyBatchState()
        {
            var existingState = Entities.UsrShopifyBatchStates.FirstOrDefault();

            if (existingState == null)
            {
                var newState = new UsrShopifyBatchState()
                {
                    ProductsEndDate = null,
                    OrdersStartDate = null,
                    OrdersEndDate = null,
                };
                Entities.UsrShopifyBatchStates.Add(newState);
            }
            else
            {
                existingState.ProductsEndDate = null;
                existingState.OrdersStartDate = null;
                existingState.OrdersEndDate = null;
            }

            Entities.SaveChanges();
        }

        public void UpdateShopifyProductsEndToNow()
        {
            var existingState = Entities.UsrShopifyBatchStates.First();
            existingState.ProductsEndDate = ComputeEndTimeStamp();
            Entities.SaveChanges();
        }

        public void UpdateForShopifyOrdersBaseline()
        {

        }
    }
}
