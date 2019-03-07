using System.Linq;
using Monster.Middle.Persist.Tenant;

namespace Monster.Middle.Processes.Sync.Persist
{
    public class PreferencesRepository
    {
        private readonly PersistContext _dataContext;

        public MonsterDataContext Entities => _dataContext.Entities;

        public PreferencesRepository(PersistContext dataContext)
        {
            _dataContext = dataContext;
        }

        static readonly object PreferencesLock = new object();

        public UsrPreference RetrievePreferences()
        {
            lock (PreferencesLock)
            {
                if (!Entities.UsrPreferences.Any())
                {
                    var preferences = new UsrPreference();
                    preferences.SyncOrdersEnabled = true;
                    preferences.SyncInventoryEnabled = true;
                    preferences.SyncRefundsEnabled = true;
                    preferences.SyncShipmentsEnabled = true;
                    preferences.MaxParallelAcumaticaSyncs = 1;

                    Entities.UsrPreferences.Add(preferences);
                    return preferences;
                }
            }

            return Entities.UsrPreferences.First();
        }

        public void SaveChanges()
        {
            this.Entities.SaveChanges();
        }
    }
}
