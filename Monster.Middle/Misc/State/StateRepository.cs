using System;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using Monster.Middle.Persist.Instance;
using Push.Foundation.Utilities.General;

namespace Monster.Middle.Misc.State
{
    public class StateRepository
    {
        private readonly MiscPersistContext _dataContext;

        public MonsterDataContext Entities => _dataContext.Entities;

        public StateRepository(MiscPersistContext dataContext)
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
                    newRecord.SettingsState = StateCode.None;
                    newRecord.SettingsTaxesState = StateCode.None;
                    newRecord.WarehouseSyncState = StateCode.None;
                    
                    newRecord.IsRandomAccessMode = false;

                    newRecord.InventoryRefreshState = StateCode.None;
                    newRecord.OrderCustomersTransPullState = StateCode.None;
                    newRecord.SyncOrdersState = StateCode.None;
                    newRecord.SyncRefundsState = StateCode.None;
                    newRecord.SyncFulfillmentsState = StateCode.None;
                    newRecord.SyncInventoryCountState = StateCode.None;

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

        public bool CheckSystemState(Func<SystemState, bool> test)
        {
            var state = RetrieveSystemStateNoTracking();
            return test(state);
        }



        public SystemState RetrieveSystemState()
        {
            CreateSystemStateIfNotExists();
            return Entities.SystemStates.First();
        }

        public void UpdateSystemState(Expression<Func<SystemState, int>> memberExpression, int newValue)
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
