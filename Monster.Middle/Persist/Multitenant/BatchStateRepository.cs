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
                existingState.ShopifyProductsPullEnd= null;
                existingState.ShopifyOrdersPullEnd = null;
                existingState.AcumaticaProductsPullEnd = null;
            }

            Entities.SaveChanges();
        }

        public void UpdateShopifyProductsPullEnd(DateTime endTimeUtc)
        {
            var existingState = RetrieveBatchState();
            existingState.ShopifyProductsPullEnd = endTimeUtc;
            Entities.SaveChanges();
        }

        public void UpdateShopifyOrdersPullEnd(DateTime endTimeUtc)
        {
            var existingState = RetrieveBatchState();
            existingState.ShopifyOrdersPullEnd = endTimeUtc;
            Entities.SaveChanges();
        }
        public void UpdateAcumaticaProductsEnd(DateTime endTimeUtc)
        {
            var existingState = RetrieveBatchState();
            existingState.AcumaticaProductsPullEnd = endTimeUtc;
            Entities.SaveChanges();
        }

        public void UpdateAcumaticaCustomersPullEnd(DateTime endTimeUtc)
        {
            var existingState = RetrieveBatchState();
            existingState.AcumaticaCustomersPullEnd = endTimeUtc;
            Entities.SaveChanges();
        }

        public UsrBatchState RetrieveBatchState()
        {
            return _dataContext.Entities.UsrBatchStates.First();
        }
    }
}
