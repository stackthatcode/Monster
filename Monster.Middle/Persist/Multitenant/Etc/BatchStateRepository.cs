using System;
using System.Linq;

namespace Monster.Middle.Persist.Multitenant.Etc
{
    public class BatchStateRepository
    {
        private readonly PersistContext _dataContext;
        public MonsterDataContext Entities => _dataContext.Entities;
        
        public BatchStateRepository(PersistContext dataContext)
        {
            _dataContext = dataContext;
        }
        

        public void ResetInventory()
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
                existingState.AcumaticaProductsPullEnd = null;
            }

            Entities.SaveChanges();
        }

        public void ResetOrders()
        {
            var existingState = Entities.UsrBatchStates.FirstOrDefault();

            if (existingState == null)
            {
                var newState = new UsrBatchState();
                Entities.UsrBatchStates.Add(newState);
            }
            else
            {
                existingState.ShopifyCustomersPullEnd = null;
                existingState.ShopifyOrdersPullEnd = null;

                existingState.AcumaticaCustomersPullEnd = null;
                existingState.AcumaticaOrdersPullEnd = null;
                existingState.AcumaticaShipmentsPullEnd = null;
            }

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

        public void UpdateAcumaticaProductsEnd(DateTime endTimeUtc)
        {
            var existingState = Retrieve();
            existingState.AcumaticaProductsPullEnd = endTimeUtc;
            Entities.SaveChanges();
        }

        public void UpdateAcumaticaCustomersPullEnd(DateTime endTimeUtc)
        {
            var existingState = Retrieve();
            existingState.AcumaticaCustomersPullEnd = endTimeUtc;
            Entities.SaveChanges();
        }

        public void UpdateAcumaticaOrdersPullEnd(DateTime endTimeUtc)
        {
            var existingState = Retrieve();
            existingState.AcumaticaOrdersPullEnd = endTimeUtc;
            Entities.SaveChanges();
        }

        public void UpdateAcumaticaShipmentsPullEnd(DateTime endTimeUtc)
        {
            var existingState = Retrieve();
            existingState.AcumaticaShipmentsPullEnd = endTimeUtc;
            Entities.SaveChanges();
        }

        public UsrBatchState Retrieve()
        {
            return _dataContext.Entities.UsrBatchStates.First();
        }
    }
}

