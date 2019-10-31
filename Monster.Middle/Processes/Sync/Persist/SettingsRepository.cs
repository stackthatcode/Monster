using System.Collections.Generic;
using System.Linq;
using Monster.Middle.Persist.Instance;


namespace Monster.Middle.Processes.Sync.Persist
{
    public class SettingsRepository
    {
        private readonly ProcessPersistContext _dataContext;

        public MonsterDataContext Entities => _dataContext.Entities;

        public SettingsRepository(ProcessPersistContext dataContext)
        {
            _dataContext = dataContext;
        }

        static readonly object PreferencesLock = new object();

        public List<PaymentGateway> RetrievePaymentGateways()
        {
            return _dataContext.Entities.PaymentGateways.ToList();
        }


        public MonsterSetting RetrieveSettings()
        {
            lock (PreferencesLock)
            {
                if (!Entities.MonsterSettings.Any())
                {
                    var preferences = new MonsterSetting();

                    preferences.SyncOrdersEnabled = true;
                    preferences.SyncInventoryEnabled = true;
                    preferences.SyncRefundsEnabled = true;
                    preferences.SyncFulfillmentsEnabled = true;
                    preferences.MaxParallelAcumaticaSyncs = 1;

                    Entities.MonsterSettings.Add(preferences);
                    return preferences;
                }
            }

            return Entities.MonsterSettings.First();
        }

        public void SaveChanges()
        {
            this.Entities.SaveChanges();
        }
    }
}
