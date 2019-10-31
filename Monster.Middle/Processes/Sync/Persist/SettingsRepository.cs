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

        static readonly object SettingsLock = new object();


        public List<PaymentGateway> RetrievePaymentGateways()
        {
            return _dataContext.Entities.PaymentGateways.ToList();
        }

        public PaymentGateway RetrievePaymentGateway(long id)
        {
            return _dataContext.Entities.PaymentGateways.FirstOrDefault(x => x.Id == id);
        }

        public PaymentGateway RetrievePaymentGatewayByShopifyId(string shopifyId)
        {
            return _dataContext
                .Entities
                .PaymentGateways
                .FirstOrDefault(x => x.ShopifyGatewayId == shopifyId);
        }

        public void ImprintPaymentGateways(IList<PaymentGateway> updatedGateways)
        {
            var existingGateways = RetrievePaymentGateways();

            foreach (var updatedGateway in updatedGateways)
            {
                if (!existingGateways.Any(x => x.ShopifyGatewayId == updatedGateway.ShopifyGatewayId))
                {
                    InsertPaymentGateway(updatedGateway);
                }
            }

            foreach (var existingGateway in existingGateways)
            {
                if (!updatedGateways.Any(x => x.ShopifyGatewayId == existingGateway.ShopifyGatewayId))
                {
                    DeletePaymentGateway(existingGateway.Id);
                }
            }

            Entities.SaveChanges();
        }

        public void InsertPaymentGateway(PaymentGateway gateway)
        {
            _dataContext.Entities.PaymentGateways.Add(gateway);
            _dataContext.Entities.SaveChanges();
        }

        public void DeletePaymentGateway(long id)
        {
            var gateway = RetrievePaymentGateway(id);
            _dataContext.Entities.PaymentGateways.Remove(gateway);
            _dataContext.Entities.SaveChanges();
        }


        public MonsterSetting RetrieveSettings()
        {
            lock (SettingsLock)
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
