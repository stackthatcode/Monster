using System;
using System.Linq;
using Monster.Middle.Persist.Instance;
using Monster.Middle.Persist.Instance;

namespace Monster.Middle.Processes.Acumatica.Persist
{
    public class AcumaticaBatchRepository
    {
        private readonly InstancePersistContext _dataContext;
        public MonsterDataContext Entities => _dataContext.Entities;

        public AcumaticaBatchRepository(InstancePersistContext dataContext)
        {
            _dataContext = dataContext;
        }


        public void Reset()
        {
            var existingState
                = Entities.AcumaticaBatchStates.FirstOrDefault();

            if (existingState != null)
            {
                Entities.AcumaticaBatchStates.Remove(existingState);
            }

            var newState = new AcumaticaBatchState();
            Entities.AcumaticaBatchStates.Add(newState);
            Entities.SaveChanges();
        }

        public void UpdateCustomersPullEnd(DateTime endTimeUtc)
        {
            var existingState = Retrieve();
            existingState.AcumaticaCustomersPullEnd = endTimeUtc;
            Entities.SaveChanges();
        }

        public void UpdateProductsEnd(DateTime endTimeUtc)
        {
            var existingState = Retrieve();
            existingState.AcumaticaStockItemPullEnd = endTimeUtc;
            Entities.SaveChanges();
        }

        public void UpdateOrdersPullEnd(DateTime endTimeUtc)
        {
            var existingState = Retrieve();
            existingState.AcumaticaOrdersPullEnd = endTimeUtc;
            Entities.SaveChanges();
        }

        public void UpdateShipmentsPullEnd(DateTime endTimeUtc)
        {
            var existingState = Retrieve();
            existingState.AcumaticaShipmentsPullEnd = endTimeUtc;
            Entities.SaveChanges();
        }
        


        public AcumaticaBatchState Retrieve()
        {
            var output =
                _dataContext
                    .Entities
                    .AcumaticaBatchStates.FirstOrDefault();

            if (output == null)
            {
                var newState = new AcumaticaBatchState();
                Entities.AcumaticaBatchStates.Add(newState);
                return newState;
            }
            else
            {
                return output;
            }
        }
    }
}

