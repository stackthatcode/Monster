using System;
using System.Collections.Generic;
using Monster.Acumatica.Api;
using Monster.Acumatica.Api.Shipment;
using Monster.Acumatica.Config;
using Monster.Middle.Persist.Instance;
using Monster.Middle.Processes.Acumatica.Persist;
using Monster.Middle.Processes.Sync.Model.Status;
using Monster.Middle.Processes.Sync.Persist;
using Monster.Middle.Utility;
using Push.Foundation.Utilities.Json;


namespace Monster.Middle.Processes.Acumatica.Workers
{
    public class AcumaticaShipmentGet
    {
        private readonly ShipmentClient _shipmentClient;
        private readonly AcumaticaOrderRepository _orderRepository;
        private readonly AcumaticaBatchRepository _batchStateRepository;
        private readonly AcumaticaTimeZoneService _timeZoneService;
        private readonly PreferencesRepository _preferencesRepository;
        private readonly AcumaticaHttpConfig _config;

        public AcumaticaShipmentGet(
                AcumaticaOrderRepository orderRepository,
                AcumaticaBatchRepository batchStateRepository,
                AcumaticaTimeZoneService timeZoneService,
                AcumaticaHttpConfig config,
                ShipmentClient shipmentClient,
                PreferencesRepository preferencesRepository)
        {
            _orderRepository = orderRepository;
            _batchStateRepository = batchStateRepository;
            _timeZoneService = timeZoneService;
            _config = config;
            _shipmentClient = shipmentClient;
            _preferencesRepository = preferencesRepository;
        }


        public void RunAutomatic()
        {
            var startOfRun = DateTime.UtcNow;
            var batchState = _batchStateRepository.Retrieve();
            var preferences = _preferencesRepository.RetrievePreferences();
            preferences.AssertStartingOrderIsValid();

            if (batchState.AcumaticaShipmentsGetEnd.HasValue)
            {
                var updateMinUtc = batchState.AcumaticaShipmentsGetEnd.Value;
                RunWithPaging(updateMinUtc);
            }
            else
            {
                var orderStartUtc = preferences.StartingShopifyOrderCreatedAtUtc.Value;
                RunWithPaging(orderStartUtc);
            }

            var batchStateEnd = (startOfRun).AddAcumaticaBatchFudge();                
            _batchStateRepository.UpdateShipmentsGetEnd(batchStateEnd);
        }

        
        private void RunWithPaging(DateTime updateMinUtc)
        {
            var updateMin = _timeZoneService.ToAcumaticaTimeZone(updateMinUtc);
            var page = 1;

            while (true)
            {
                var json = _shipmentClient
                    .RetrieveShipments(
                        updateMin, ShipmentExpand.Details, page, _config.PageSize);

                var shipments = json.DeserializeFromJson<List<Shipment>>();
                UpsertShipmentsToPersist(shipments);

                // Need to do this because Acumatica constrains the number of Details
                // ... that can be expanded per call
                //
                var packagesJson
                    = _shipmentClient.RetrieveShipments(
                        updateMin, ShipmentExpand.Packages, page, _config.PageSize);

                var shipmentsWithPackages = packagesJson.DeserializeFromJson<List<Shipment>>();
                shipments.AppendPackageRefs(shipmentsWithPackages);

                UpsertShipmentsToPersist(shipments);

                if (shipments.Count == 0)
                {
                    break;
                }
                page++;
            }
        }

        public void UpsertShipmentsToPersist(List<Shipment> shipments)
        {
            foreach (var shipment in shipments)
            {
                using (var transaction = _orderRepository.BeginTransaction())
                {
                    UpsertShipmentToPersist(shipment);
                    transaction.Commit();
                }
            }
        }
        
        public AcumaticaShipment UpsertShipmentToPersist(Shipment shipment)
        {
            var shipmentNbr = shipment.ShipmentNbr.value;
            var existingData = _orderRepository.RetrieveShipment(shipmentNbr);

            if (existingData == null)
            {
                var newData = new AcumaticaShipment();
                newData.AcumaticaJson = shipment.SerializeToJson();
                newData.AcumaticaShipmentNbr = shipment.ShipmentNbr.value;
                newData.AcumaticaStatus = shipment.Status.value;
                newData.AcumaticaTrackingNbr = shipment.LeadTrackingNbr;
                newData.DateCreated = DateTime.UtcNow;
                newData.LastUpdated = DateTime.UtcNow;

                _orderRepository.InsertShipment(newData);

                UpsertShipmentSalesOrderRefs(newData.Id, shipment);
                return newData;
            }
            else
            {
                existingData.AcumaticaJson = shipment.SerializeToJson();
                existingData.AcumaticaStatus = shipment.Status.value;
                existingData.AcumaticaTrackingNbr = shipment.LeadTrackingNbr;
                existingData.LastUpdated = DateTime.UtcNow;

                _orderRepository.SaveChanges();

                UpsertShipmentSalesOrderRefs(existingData.Id, shipment);
                
                return existingData;
            }
        }
        
        public void UpsertShipmentSalesOrderRefs(
                    long monsterShipmentId, Shipment shipment)
        {
            var currentDetailRecords = new List<AcumaticaShipmentSalesOrderRef>();

            foreach (var detail in shipment.Details)
            {
                var isMonsterSyncedOrder 
                        = _orderRepository.IsMonsterSyncedOrder(detail.OrderNbr.value);
                    
                var currentDetailRecord =
                    new AcumaticaShipmentSalesOrderRef
                    {
                        ShipmentMonsterId = monsterShipmentId,
                        AcumaticaShipmentNbr = shipment.ShipmentNbr.value,
                        AcumaticaOrderNbr = detail.OrderNbr.value,
                        IsMonsterOrder = isMonsterSyncedOrder,
                    };

                currentDetailRecords.Add(currentDetailRecord);
            }

            _orderRepository.ImprintShipmentOrderRefs(monsterShipmentId, currentDetailRecords);
        }
    }
}

