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

        private readonly TenantRepository _tenantRepository;
        private readonly AcumaticaBatchRepository _batchStateRepository;
        private readonly TimeZoneService _timeZoneService;
        private readonly IPushLogger _logger;

        public const int InitialBatchStateFudgeMin = -15;


        public AcumaticaShipmentPull(
                AcumaticaOrderRepository orderRepository,
                AcumaticaCustomerPull acumaticaCustomerPull,
                AcumaticaBatchRepository batchStateRepository,
                TimeZoneService timeZoneService,
                ShipmentClient shipmentClient,
                TenantRepository tenantRepository,
                IPushLogger logger)
        {
            _acumaticaCustomerPull = acumaticaCustomerPull;
            _orderRepository = orderRepository;
            _batchStateRepository = batchStateRepository;
            _timeZoneService = timeZoneService;
            _shipmentClient = shipmentClient;
            _tenantRepository = tenantRepository;
            _logger = logger;
        }


        public void RunAutomatic()
        {
            // Pull Shipments that are detected from Sales Orders
            RunStubbedShipments();

            // Pull Shipments based on Batch State
            var batchState = _batchStateRepository.Retrieve();
            if (batchState.AcumaticaShipmentsPullEnd.HasValue)
            {
                RunUpdated();
            }
            else
            {
                RunAll();
            }
        }

        private void RunAll()
        {
            var preferences = _tenantRepository.RetrievePreferences();
            var shipmentUpdateMin = preferences.DataPullStart;

            var json = _shipmentClient.RetrieveShipments(shipmentUpdateMin);

            var shipments = json.DeserializeFromJson<List<Shipment>>();

            UpsertShipmentsToPersist(shipments);

            // Set the Batch State Pull End marker
            var maxOrderDate =
                _orderRepository.RetrieveShipmentMaxUpdatedDate();

            var batchStateEnd
                = maxOrderDate
                    ?? DateTime.UtcNow.AddMinutes(InitialBatchStateFudgeMin);

            _batchStateRepository.UpdateOrderShipmentsPullEnd(batchStateEnd);
        }
        
        private void RunUpdated()
        {
            var batchState = _batchStateRepository.Retrieve();
            var updateMinUtc = batchState.AcumaticaShipmentsPullEnd;
            var updateMin = _timeZoneService.ToAcumaticaTimeZone(updateMinUtc.Value);

            var pullRunStartTime = DateTime.UtcNow;

            var json = _shipmentClient.RetrieveShipments(updateMin);
            var shipments = json.DeserializeFromJson<List<Shipment>>();

            UpsertShipmentsToPersist(shipments);

            _batchStateRepository.UpdateOrderShipmentsPullEnd(pullRunStartTime);
        }

        private void RunStubbedShipments()
        {
            var stubbedShipments = _orderRepository.RetrieveShipmentStubs();

            foreach (var stubbedShipment in stubbedShipments)
            {
                var json 
                    = _shipmentClient
                        .RetrieveShipment(stubbedShipment.AcumaticaShipmentNbr);

                var shipment = json.DeserializeFromJson<Shipment>();

                UpsertShipmentToPersist(shipment, false);
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
        
        public UsrAcumaticaShipment 
                UpsertShipmentToPersist(
                    Shipment shipment, bool isCreatedByMonster = false)
        {
            var shipmentNbr = shipment.ShipmentNbr.value;

            var existingData
                    = _orderRepository.RetrieveShipment(shipmentNbr);

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
            var currentDetailRecords = new List<UsrAcumaticaShipmentDetail>();

            foreach (var detail in shipment.Details)
            {
                var currentDetailRecord =
                    new UsrAcumaticaShipmentDetail
                    {
                        ShipmentMonsterId = monsterShipmentId,
                        AcumaticaShipmentNbr = shipment.ShipmentNbr.value,
                        AcumaticaOrderNbr = detail.OrderNbr.value,
                    };

                currentDetailRecords.Add(currentDetailRecord);
            }

            _orderRepository
                .ImprintShipmentDetail(
                    monsterShipmentId, currentDetailRecords);
        }
    }
}

