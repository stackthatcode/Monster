using System;
using System.Linq;
using Monster.Middle.Persist.Multitenant;

namespace Monster.Middle.Processes.Acumatica.Persist
{
    public class AcumaticaBatchRepository
    {
        private readonly PersistContext _dataContext;
        public MonsterDataContext Entities => _dataContext.Entities;

        public AcumaticaBatchRepository(PersistContext dataContext)
        {
            _dataContext = dataContext;
        }


        public void Reset()
        {
            var existingState
                = Entities.UsrAcumaticaBatchStates.FirstOrDefault();

            if (existingState != null)
            {
                Entities.UsrAcumaticaBatchStates.Remove(existingState);
            }

            var newState = new UsrAcumaticaBatchState();
            Entities.UsrAcumaticaBatchStates.Add(newState);
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
            existingState.AcumaticaProductsPullEnd = endTimeUtc;
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
        


        public UsrAcumaticaBatchState Retrieve()
        {
            var output =
                _dataContext
                    .Entities
                    .UsrAcumaticaBatchStates.FirstOrDefault();

            if (output == null)
            {
                var newState = new UsrAcumaticaBatchState();
                Entities.UsrAcumaticaBatchStates.Add(newState);
                return newState;
            }
            else
            {
                return output;
            }
        }
    }
}

