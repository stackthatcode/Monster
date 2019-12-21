using System.Collections.Generic;
using System.Linq;
using Monster.Middle.Misc.Hangfire;
using Monster.Middle.Persist.Instance;


namespace Monster.Middle.Processes.Sync.Persist
{
    public class SettingsRepository
    {
        private readonly ProcessPersistContext _dataContext;

        public const int DefaultMaxNumberOfOrders = 5000;


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

        public bool GatewayExistsInConfig(string shopifyGatewayId)
        {
            return _dataContext.Entities.PaymentGateways.Any(x => x.ShopifyGatewayId == shopifyGatewayId);
        }


        public MonsterSetting RetrieveSettings()
        {
            lock (SettingsLock)
            {
                if (!Entities.MonsterSettings.Any())
                {
                    var settings = new MonsterSetting();

                    settings.SyncOrdersEnabled = false;
                    settings.SyncInventoryEnabled = false;
                    settings.SyncRefundsEnabled = false;
                    settings.SyncFulfillmentsEnabled = false;
                    settings.MaxParallelAcumaticaSyncs = 1;
                    settings.MaxNumberOfOrders = 1;
                    settings.InventorySyncWeight = true;
                    settings.InventorySyncPrice = true;
                    settings.InventorySyncAvailableQty = true;
                    settings.LastRecurringSchedule = RecurringSchedule.Default.Id;
                    Entities.MonsterSettings.Add(settings);
                    return settings;
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
