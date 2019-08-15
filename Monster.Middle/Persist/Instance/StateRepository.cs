using System;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using Monster.Middle.Processes.Sync.Model.Misc;
using Push.Foundation.Utilities.General;

namespace Monster.Middle.Persist.Instance
{
    public class StateRepository
    {
        private readonly PersistContext _dataContext;

        public MonsterDataContext Entities => _dataContext.Entities;

        public StateRepository(PersistContext dataContext)
        {
            _dataContext = dataContext;
        }

        public DbContextTransaction BeginTransaction()
        {
            return Entities.Database.BeginTransaction();
        }


        static readonly object SystemStateLock = new object();

        public void CreateSystemStateIfNotExists()
        {
            lock (SystemStateLock)
            {
                if (!Entities.SystemStates.Any())
                {
                    var newRecord = new SystemState();

                    newRecord.ShopifyConnState = StateCode.None;
                    newRecord.AcumaticaConnState = StateCode.None;
                    newRecord.AcumaticaRefDataState = StateCode.None;
                    newRecord.PreferenceState = StateCode.None;
                    newRecord.WarehouseSyncState = StateCode.None;
                    newRecord.InventoryPullState = StateCode.None;
                    
                    newRecord.IsRandomAccessMode = false;
                    newRecord.RealTimeHangFireJobId = null;

                    Entities.SystemStates.Add(newRecord);
                    Entities.SaveChanges();
                }
            }
        }
        
        public SystemState RetrieveSystemStateNoTracking()
        {
            CreateSystemStateIfNotExists();
            return Entities.SystemStates.AsNoTracking().First();
        }

        public SystemState RetrieveSystemState()
        {
            CreateSystemStateIfNotExists();
            return Entities.SystemStates.First();
        }

        public void UpdateSystemState(
                Expression<Func<SystemState, int>> memberExpression, int newValue)
        {
            CreateSystemStateIfNotExists();
            var state = Entities.SystemStates.First();
            state.SetPropertyValue(memberExpression, newValue);
            Entities.SaveChanges();
        }

        public void UpdateSystemState(
            Expression<Func<SystemState, bool>> memberExpression, bool newValue)
        {
            CreateSystemStateIfNotExists();
            var state = Entities.SystemStates.First();
            state.SetPropertyValue(memberExpression, newValue);
            Entities.SaveChanges();
        }

        public void UpdateIsRandomAccessMode(bool newValue)
        {
            CreateSystemStateIfNotExists();
            var state = Entities.SystemStates.First();
            state.IsRandomAccessMode = newValue;
            Entities.SaveChanges();
        }
        
        public void SaveChanges()
        {
            this.Entities.SaveChanges();
        }
    }
}
