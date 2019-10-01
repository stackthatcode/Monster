using System.Linq;
using Monster.Middle.Persist.Instance;
using Monster.Middle.Persist.Instance;

namespace Monster.Middle.Processes.Sync.Persist
{
    public class PreferencesRepository
    {
        private readonly InstancePersistContext _dataContext;

        public MonsterDataContext Entities => _dataContext.Entities;

        public PreferencesRepository(InstancePersistContext dataContext)
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
                    preferences.SyncFulfillmentsEnabled = true;
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
