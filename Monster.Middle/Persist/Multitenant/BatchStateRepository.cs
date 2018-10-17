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
        

        public void ResetBatchState()
        {
            var existingState = Entities.UsrBatchStates.FirstOrDefault();

            if (existingState == null)
            {
                var newState = new UsrBatchState();

                Entities.UsrBatchStates.Add(newState);
            }
            else
            {
                existingState.ShopifyProductsEndDate = null;
                existingState.ShopifyOrdersStartDate = null;
                existingState.ShopifyOrdersEndDate = null;
                existingState.AcumaticaProductsEndDate = null;
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

        public void UpdateAcumaticaProductsEnd(DateTime endTimeUtc)
        {
            var existingState = Entities.UsrBatchStates.First();
            existingState.AcumaticaProductsEndDate = endTimeUtc;
            Entities.SaveChanges();
        }

        public UsrBatchState RetrieveBatchState()
        {
            return _dataContext.Entities.UsrBatchStates.First();
        }
    }
}
