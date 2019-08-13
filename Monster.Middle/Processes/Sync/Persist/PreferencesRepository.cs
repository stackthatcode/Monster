using System.Linq;
using Monster.Middle.Persist.Instance;
using Monster.Middle.Persist.Instance;

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

        public Preference RetrievePreferences()
        {
            lock (PreferencesLock)
            {
                if (!Entities.Preferences.Any())
                {
                    var preferences = new Preference();
                    preferences.SyncOrdersEnabled = true;
                    preferences.SyncInventoryEnabled = true;
                    preferences.SyncRefundsEnabled = true;
                    preferences.SyncShipmentsEnabled = true;
                    preferences.MaxParallelAcumaticaSyncs = 1;

                    Entities.Preferences.Add(preferences);
                    return preferences;
                }
            }

            return Entities.Preferences.First();
        }

        public void SaveChanges()
        {
            this.Entities.SaveChanges();
        }
    }
}
