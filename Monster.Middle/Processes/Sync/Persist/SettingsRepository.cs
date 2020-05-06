using System.Collections.Generic;
using System.Linq;
using Monster.Middle.Misc.Hangfire;
using Monster.Middle.Persist.Instance;
using Monster.Middle.Processes.Sync.Model.Reference;


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



        // Payment Gateways
        //
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



        // Rate-to-Ship-Via
        //
        public List<RateToShipVia> RetrieveRateToShipVias()
        {
            return _dataContext.Entities.RateToShipVias.ToList();
        }

        public RateToShipVia RetrieveRateToShipVia(string shopifyRateName)
        {
            return _dataContext
                .Entities
                .RateToShipVias
                .FirstOrDefault(x => x.ShopifyRateName == shopifyRateName);
        }

        public RateToShipVia RetrieveRateToShipViaByAcumaticaId(string acumaticaRateId)
        {
            return _dataContext
                .Entities
                .RateToShipVias
                .FirstOrDefault(x => x.AcumaticaShipViaId == acumaticaRateId);
        }

        public void ImprintRateToShipVias(IList<RateToShipVia> updatedRecords)
        {
            var existingRecords = RetrieveRateToShipVias();

            foreach (var updatedRecord in updatedRecords)
            {
                if (existingRecords.All(x => x.ShopifyRateName != updatedRecord.ShopifyRateName))
                {
                    InsertRateToShipVia(updatedRecord);
                }
            }

            foreach (var existingRecord in existingRecords)
            {
                if (updatedRecords.All(x => x.ShopifyRateName != existingRecord.ShopifyRateName))
                {
                    DeleteRateToShipVia(existingRecord.ShopifyRateName);
                }
            }

            Entities.SaveChanges();
        }

        public void InsertRateToShipVia(RateToShipVia gateway)
        {
            _dataContext.Entities.RateToShipVias.Add(gateway);
            _dataContext.Entities.SaveChanges();
        }

        public void DeleteRateToShipVia(string shopifyRateName)
        {
            var records = RetrieveRateToShipVia(shopifyRateName);
            _dataContext.Entities.RateToShipVias.Remove(records);
            _dataContext.Entities.SaveChanges();
        }

        public bool RateMappingExists(string shopifyRateName)
        {
            return _dataContext
                .Entities
                .RateToShipVias
                .Any(x => x.ShopifyRateName == shopifyRateName);
        }



        public MonsterSetting RetrieveSettings()
        {
            lock (SettingsLock)
            {
                if (!Entities.MonsterSettings.Any())
                {
                    var settings = new MonsterSetting();

                    settings.PullFromShopifyEnabled = false;
                    settings.PullFromAcumaticaEnabled = false;

                    settings.SyncOrdersEnabled = false;
                    settings.SyncAllCustomersEnabled = false;
                    settings.SyncInventoryEnabled = false;
                    settings.SyncRefundsEnabled = false;
                    settings.SyncFulfillmentsEnabled = false;
                    settings.MaxParallelAcumaticaSyncs = 1;
                    settings.MaxNumberOfOrders = 5000;
                    settings.ReleasePaymentsOnSync = false;

                    settings.ShopifyDelayMs = ShopifyDelay.Default;
                    settings.InventorySyncWeight = true;
                    settings.InventorySyncPrice = true;
                    settings.InventorySyncAvailableQty = true;

                    settings.LastRecurringSchedule = RecurringSchedule.Default.Id;
                    Entities.MonsterSettings.Add(settings);
                    Entities.SaveChanges();
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
