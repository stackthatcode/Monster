using System;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using Monster.Middle.Persist.Tenant;
using Monster.Middle.Processes.Sync.Model.Misc;
using Push.Foundation.Utilities.General;

namespace Monster.Middle.Processes.Sync.Services
{
    public class SystemStateRepository
    {
        private readonly PersistContext _dataContext;

        public MonsterDataContext Entities => _dataContext.Entities;

        public SystemStateRepository(PersistContext dataContext)
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
                if (!Entities.UsrSystemStates.Any())
                {
                    var newRecord = new UsrSystemState();

                    newRecord.ShopifyConnState = SystemState.None;
                    newRecord.AcumaticaConnState = SystemState.None;
                    newRecord.AcumaticaRefDataState = SystemState.None;
                    newRecord.PreferenceState = SystemState.None;
                    newRecord.WarehouseSyncState = SystemState.None;
                    newRecord.InventoryPullState = SystemState.None;
                    
                    newRecord.IsShopifyUrlFinalized = false;
                    newRecord.IsAcumaticaUrlFinalized = false;
                    newRecord.IsRandomAccessMode = false;
                    newRecord.RealTimeHangFireJobId = null;

                    Entities.UsrSystemStates.Add(newRecord);
                    Entities.SaveChanges();
                }
            }
        }
        
        public UsrSystemState RetrieveSystemStateNoTracking()
        {
            CreateSystemStateIfNotExists();
            return Entities.UsrSystemStates.AsNoTracking().First();
        }

        public UsrSystemState RetrieveSystemState()
        {
            CreateSystemStateIfNotExists();
            return Entities.UsrSystemStates.First();
        }

        public void UpdateSystemState(
                Expression<Func<UsrSystemState, int>> memberExpression, int newValue)
        {
            CreateSystemStateIfNotExists();
            var state = Entities.UsrSystemStates.First();
            state.SetPropertyValue(memberExpression, newValue);
            Entities.SaveChanges();
        }

        public void UpdateIsRandomAccessMode(bool newValue)
        {
            CreateSystemStateIfNotExists();
            var state = Entities.UsrSystemStates.First();
            state.IsRandomAccessMode = newValue;
            Entities.SaveChanges();
        }
        
        public void SaveChanges()
        {
            this.Entities.SaveChanges();
        }
    }
}
