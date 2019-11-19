using System;
using System.Linq;
using Monster.Middle.Persist.Instance;

namespace Monster.Middle.Processes.Acumatica.Persist
{
    public class AcumaticaBatchRepository
    {
        private readonly ProcessPersistContext _dataContext;
        public MonsterDataContext Entities => _dataContext.Entities;

        public AcumaticaBatchRepository(ProcessPersistContext dataContext)
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

        public void UpdateCustomersGetEnd(DateTime endTimeUtc)
        {
            var existingState = Retrieve();
            existingState.AcumaticaCustomersGetEnd = endTimeUtc;
            Entities.SaveChanges();
        }

        public void UpdateStockItemGetEnd(DateTime endTimeUtc)
        {
            var existingState = Retrieve();
            existingState.AcumaticaStockItemGetEnd = endTimeUtc;
            Entities.SaveChanges();
        }

        public void UpdateInventoryStatusGetEnd(DateTime endTimeUtc)
        {
            var existingState = Retrieve();
            existingState.AcumaticaInventoryStatusGetEnd = endTimeUtc;
            Entities.SaveChanges();
        }

        public void UpdateOrdersGetEnd(DateTime endTimeUtc)
        {
            var existingState = Retrieve();
            existingState.AcumaticaOrdersGetEnd = endTimeUtc;
            Entities.SaveChanges();
        }

        public void UpdateShipmentsGetEnd(DateTime endTimeUtc)
        {
            var existingState = Retrieve();
            existingState.AcumaticaShipmentsGetEnd = endTimeUtc;
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

