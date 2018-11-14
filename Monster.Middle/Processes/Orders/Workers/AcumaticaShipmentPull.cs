using System;
using System.Collections.Generic;
using Monster.Acumatica.Api;
using Monster.Acumatica.Api.Shipment;
using Monster.Middle.Persist.Multitenant;
using Monster.Middle.Persist.Multitenant.Acumatica;
using Monster.Middle.Persist.Multitenant.Etc;
using Monster.Middle.Services;
using Push.Foundation.Utilities.Json;
using Push.Foundation.Utilities.Logging;


namespace Monster.Middle.Processes.Orders.Workers
{
    public class AcumaticaShipmentPull
    {
        private readonly ShipmentClient _shipmentClient;
        private readonly AcumaticaOrderRepository _acumaticaOrderRepository;
        private readonly AcumaticaCustomerPull _acumaticaCustomerPull;

        private readonly TenantRepository _tenantRepository;
        private readonly BatchStateRepository _batchStateRepository;
        private readonly TimeZoneService _timeZoneService;
        private readonly IPushLogger _logger;

        public const int InitialBatchStateFudgeMin = -15;


        public AcumaticaShipmentPull(
                AcumaticaOrderRepository acumaticaOrderRepository,
                AcumaticaCustomerPull acumaticaCustomerPull,
                BatchStateRepository batchStateRepository,
                TimeZoneService timeZoneService,
                ShipmentClient shipmentClient,
                TenantRepository tenantRepository,
                IPushLogger logger)
        {
            _acumaticaCustomerPull = acumaticaCustomerPull;
            _acumaticaOrderRepository = acumaticaOrderRepository;
            _batchStateRepository = batchStateRepository;
            _timeZoneService = timeZoneService;
            _shipmentClient = shipmentClient;
            _tenantRepository = tenantRepository;
            _logger = logger;
        }


        public void RunAutomatic()
        {
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
                _acumaticaOrderRepository.RetrieveShipmentMaxUpdatedDate();

            var batchStateEnd
                = maxOrderDate
                    ?? DateTime.UtcNow.AddMinutes(InitialBatchStateFudgeMin);

            _batchStateRepository
                .UpdateAcumaticaShipmentsPullEnd(batchStateEnd);
        }

        private void RunUpdated()
        {
            var batchState = _batchStateRepository.Retrieve();
            if (!batchState.AcumaticaShipmentsPullEnd.HasValue)
            {
                throw new Exception(
                    "AcumaticaOrdersPullEnd is null - execute RunAll() first");
            }

            var updateMinUtc = batchState.AcumaticaShipmentsPullEnd;
            var updateMin = _timeZoneService.ToAcumaticaTimeZone(updateMinUtc.Value);

            var pullRunStartTime = DateTime.UtcNow;

            var json = _shipmentClient.RetrieveShipments(updateMin);
            var shipments = json.DeserializeFromJson<List<Shipment>>();

            UpsertShipmentsToPersist(shipments);

            _batchStateRepository.UpdateAcumaticaShipmentsPullEnd(pullRunStartTime);
        }

        public void UpsertShipmentsToPersist(List<Shipment> shipments)
        {
            foreach (var shipment in shipments)
            {
                UpsertShipmentToPersist(shipment);
            }
        }

        public UsrAcumaticaShipment 
                    UpsertShipmentToPersist(Shipment shipment)
        {
            var shipmentNbr = shipment.ShipmentNbr.value;

            var existingData
                = _acumaticaOrderRepository
                    .RetrieveShipment(shipmentNbr);

            if (existingData == null)
            {
                var newData = new UsrAcumaticaShipment();
                newData.AcumaticaJson = shipment.SerializeToJson();
                newData.AcumaticaShipmentNbr = shipment.ShipmentNbr.value;
                newData.AcumaticaStatus = shipment.Status.value;
                newData.DateCreated = DateTime.UtcNow;
                newData.LastUpdated = DateTime.UtcNow;

                using (var transaction = _acumaticaOrderRepository.BeginTransaction())
                {
                    _acumaticaOrderRepository.InsertShipment(newData);
                    UpsertSOShipments(newData.Id, shipment);
                    transaction.Commit();
                }

                return newData;
            }
            else
            {
                existingData.AcumaticaJson = shipment.SerializeToJson();
                existingData.AcumaticaStatus = shipment.Status.value;
                existingData.LastUpdated = DateTime.UtcNow;

                using (var transaction = _acumaticaOrderRepository.BeginTransaction())
                {
                    _acumaticaOrderRepository.SaveChanges();
                    UpsertSOShipments(existingData.Id, shipment);
                    transaction.Commit();
                }

                return existingData;
            }
        }

        public void UpsertSOShipments(long monsterShipmentId, Shipment shipment)
        {
            var currentDetailRecords = new List<UsrAcumaticaShipmentSo>();

            foreach (var detail in shipment.Details)
            {
                var currentDetailRecord =
                    new UsrAcumaticaShipmentSo
                    {
                        ShipmentMonsterId = monsterShipmentId,
                        AcumaticaShipmentNbr = shipment.ShipmentNbr.value,
                        AcumaticaOrderNbr = detail.OrderNbr.value,
                    };

                currentDetailRecords.Add(currentDetailRecord);
            }

            _acumaticaOrderRepository
                .ImprintShipmentDetail(
                    monsterShipmentId, currentDetailRecords);
        }
    }
}

