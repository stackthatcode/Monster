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



        // Carrier-to-Ship-Via
        //
        public List<CarrierToShipVia> RetrieveCarrierToShipVias()
        {
            return _dataContext.Entities.CarrierToShipVias.ToList();
        }

        public CarrierToShipVia RetrieveCarrierToShipVia(string shopifyCarrierName)
        {
            return _dataContext
                .Entities
                .CarrierToShipVias
                .FirstOrDefault(x => x.ShopifyCarrierName == shopifyCarrierName);
        }

        public CarrierToShipVia RetrieveCarrierToShipViaByAcumaticaId(string acumaticaCarrierId)
        {
            return _dataContext
                .Entities
                .CarrierToShipVias
                .FirstOrDefault(x => x.AcumaticaCarrierId == acumaticaCarrierId);
        }

        public void ImprintCarrierToShipVias(IList<CarrierToShipVia> updatedRecords)
        {
            var existingRecords = RetrieveCarrierToShipVias();

            foreach (var updatedRecord in updatedRecords)
            {
                if (existingRecords.All(x => x.ShopifyCarrierName != updatedRecord.ShopifyCarrierName))
                {
                    InsertCarrierToShipVia(updatedRecord);
                }
            }

            foreach (var existingRecord in existingRecords)
            {
                if (!updatedRecords.Any(x => x.ShopifyCarrierName == existingRecord.ShopifyCarrierName))
                {
                    DeleteCarrierToShipVia(existingRecord.ShopifyCarrierName);
                }
            }

            Entities.SaveChanges();
        }

        public void InsertCarrierToShipVia(CarrierToShipVia gateway)
        {
            _dataContext.Entities.CarrierToShipVias.Add(gateway);
            _dataContext.Entities.SaveChanges();
        }

        public void DeleteCarrierToShipVia(string shopifyCarrierName)
        {
            var records = RetrieveCarrierToShipVia(shopifyCarrierName);
            _dataContext.Entities.CarrierToShipVias.Remove(records);
            _dataContext.Entities.SaveChanges();
        }

        public bool CarrierMappingExists(string shopifyCarrierName)
        {
            return _dataContext
                .Entities
                .CarrierToShipVias
                .Any(x => x.ShopifyCarrierName == shopifyCarrierName);
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
