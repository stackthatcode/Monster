using System.Linq;
using Monster.Middle.Persist.Tenant;

namespace Monster.Middle.Processes.Sync.Services
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
                    preferences.FulfillmentInAcumatica = true;
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
