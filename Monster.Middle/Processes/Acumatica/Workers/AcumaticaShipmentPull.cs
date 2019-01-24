using System;
using System.Collections.Generic;
using Monster.Acumatica.Api;
using Monster.Acumatica.Api.Shipment;
using Monster.Middle.Persist.Multitenant;
using Monster.Middle.Processes.Acumatica.Persist;
using Monster.Middle.Services;
using Push.Foundation.Utilities.Json;
using Push.Foundation.Utilities.Logging;

namespace Monster.Middle.Processes.Acumatica.Workers
{
    public class AcumaticaShipmentPull
    {
        private readonly ShipmentClient _shipmentClient;
        private readonly AcumaticaOrderRepository _orderRepository;
        private readonly AcumaticaCustomerPull _acumaticaCustomerPull;

        private readonly ConnectionRepository _connectionRepository;
        private readonly AcumaticaBatchRepository _batchStateRepository;
        private readonly InstanceTimeZoneService _timeZoneService;
        private readonly IPushLogger _logger;
        private readonly PreferencesRepository _preferencesRepository;

        public const int InitialBatchStateFudgeMin = -15;


        public AcumaticaShipmentPull(
                AcumaticaOrderRepository orderRepository,
                AcumaticaCustomerPull acumaticaCustomerPull,
                AcumaticaBatchRepository batchStateRepository,
                InstanceTimeZoneService timeZoneService,
                ShipmentClient shipmentClient,
                ConnectionRepository connectionRepository,
                IPushLogger logger, 
                PreferencesRepository preferencesRepository)
        {
            _acumaticaCustomerPull = acumaticaCustomerPull;
            _orderRepository = orderRepository;
            _batchStateRepository = batchStateRepository;
            _timeZoneService = timeZoneService;
            _shipmentClient = shipmentClient;
            _connectionRepository = connectionRepository;
            _logger = logger;
            _preferencesRepository = preferencesRepository;
        }


        public void RunAutomatic()
        {
            var startOfRun = DateTime.UtcNow;
            var batchState = _batchStateRepository.Retrieve();

            if (batchState.AcumaticaShipmentsPullEnd.HasValue)
            {
                RunUpdated();
            }
            else
            {
                RunAll();
            }

            var maxOrderDate = _orderRepository.RetrieveShipmentMaxUpdatedDate();
            var batchStateEnd = (maxOrderDate ?? startOfRun).AddAcumaticaBatchFudge();                
            _batchStateRepository.UpdateShipmentsPullEnd(batchStateEnd);
        }

        private void RunAll()
        {
            var preferences = _preferencesRepository.RetrievePreferences();
            var orderStart = preferences.ShopifyOrderDateStart.Value;
            var shipmentUpdateMin = _timeZoneService.ToAcumaticaTimeZone(orderStart);

            var json = _shipmentClient.RetrieveShipments(shipmentUpdateMin);
            var shipments = json.DeserializeFromJson<List<Shipment>>();

            UpsertShipmentsToPersist(shipments);            
        }
        
        private void RunUpdated()
        {
            var batchState = _batchStateRepository.Retrieve();
            var updateMinUtc = batchState.AcumaticaShipmentsPullEnd;
            var updateMin = _timeZoneService.ToAcumaticaTimeZone(updateMinUtc.Value);

            var json = _shipmentClient.RetrieveShipments(updateMin);
            var shipments = json.DeserializeFromJson<List<Shipment>>();

            UpsertShipmentsToPersist(shipments);
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
        
        public UsrAcumaticaShipment 
                UpsertShipmentToPersist(
                        Shipment shipment, bool isCreatedByMonster = false)
        {
            var shipmentNbr = shipment.ShipmentNbr.value;
            var existingData = _orderRepository.RetrieveShipment(shipmentNbr);

            if (existingData == null)
            {
                var newData = new UsrAcumaticaShipment();
                newData.AcumaticaJson = shipment.SerializeToJson();
                newData.AcumaticaShipmentNbr = shipment.ShipmentNbr.value;
                newData.AcumaticaStatus = shipment.Status.value;
                newData.IsCreatedByMonster = isCreatedByMonster;
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
                existingData.LastUpdated = DateTime.UtcNow;

                _orderRepository.SaveChanges();
                UpsertShipmentSalesOrderRefs(existingData.Id, shipment);
                
                return existingData;
            }
        }
        
        public void UpsertShipmentSalesOrderRefs(
                    long monsterShipmentId, Shipment shipment)
        {
            var currentDetailRecords 
                    = new List<UsrAcumaticaShipmentSalesOrderRef>();

            foreach (var detail in shipment.Details)
            {
                var isMonsterSyncedOrder 
                        =  _orderRepository.IsMonsterSyncedOrder(detail.OrderNbr.value);
                    
                var currentDetailRecord =
                    new UsrAcumaticaShipmentSalesOrderRef
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

