using System;
using System.Collections.Generic;
using System.Linq;
using Monster.Acumatica.Api;
using Monster.Acumatica.Api.Shipment;
using Monster.Middle.Persist.Multitenant;
using Monster.Middle.Persist.Multitenant.Acumatica;
using Monster.Middle.Persist.Multitenant.Etc;
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
        private readonly IPushLogger _logger;

        public const int InitialBatchStateFudgeMin = -15;


        public AcumaticaShipmentPull(
                AcumaticaOrderRepository acumaticaOrderRepository,
                AcumaticaCustomerPull acumaticaCustomerPull,
                BatchStateRepository batchStateRepository,
                ShipmentClient shipmentClient,
                TenantRepository tenantRepository,
                IPushLogger logger)
        {
            _acumaticaCustomerPull = acumaticaCustomerPull;
            _acumaticaOrderRepository = acumaticaOrderRepository;
            _batchStateRepository = batchStateRepository;
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

            var updateMin = batchState.AcumaticaShipmentsPullEnd;
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
                var customerMonsterId =
                    _acumaticaCustomerPull.RunAndUpsertCustomer(
                        shipment.CustomerID.value);

                var newData = new UsrAcumaticaShipment();
                newData.AcumaticaJson = shipment.SerializeToJson();
                newData.AcumaticaShipmentId = shipment.ShipmentNbr.value;
                newData.CustomerMonsterId = customerMonsterId;

                newData.DateCreated = DateTime.UtcNow;
                newData.LastUpdated = DateTime.UtcNow;

                _acumaticaOrderRepository.InsertShipment(newData);

                UpsertSOShipments(shipment);

                return newData;
            }
            else
            {
                existingData.AcumaticaJson = shipment.SerializeToJson();
                existingData.LastUpdated = DateTime.UtcNow;

                _acumaticaOrderRepository.SaveChanges();
                
                UpsertSOShipments(shipment);

                return existingData;
            }
        }

        public void UpsertSOShipments(Shipment shipment)
        {
            var currentDetailRecords = new List<UsrAcumaticaSoShipment>();
            
            foreach (var detail in shipment.Details)
            {
                var currentDetailRecord =
                    new UsrAcumaticaSoShipment
                    {
                        AcumaticaSalesOrderId = detail.OrderNbr.value,
                        AcumaticaShipmentId = shipment.ShipmentNbr.value
                    };
                
                currentDetailRecords.Add(currentDetailRecord);
            }

            _acumaticaOrderRepository
                .ImprintShipmentDetail(
                    shipment.ShipmentNbr.value, currentDetailRecords);
        }

    }
}

