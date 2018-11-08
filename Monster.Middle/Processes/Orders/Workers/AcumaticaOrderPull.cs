using System;
using System.Collections.Generic;
using Monster.Acumatica.Api;
using Monster.Acumatica.Api.SalesOrder;
using Monster.Middle.Persist.Multitenant;
using Monster.Middle.Persist.Multitenant.Acumatica;
using Monster.Middle.Persist.Multitenant.Etc;
using Push.Foundation.Utilities.Json;
using Push.Foundation.Utilities.Logging;


namespace Monster.Middle.Processes.Orders.Workers
{
    public class AcumaticaOrderPull
    {
        private readonly CustomerClient _customerClient;
        private readonly SalesOrderClient _salesOrderClient;
        private readonly AcumaticaOrderRepository _acumaticaOrderRepository;
        private readonly AcumaticaCustomerPull _acumaticaCustomerPull;
        private readonly TenantRepository _tenantRepository;
        private readonly BatchStateRepository _batchStateRepository;
        private readonly IPushLogger _logger;

        public const int InitialBatchStateFudgeMin = -15;


        public AcumaticaOrderPull(
                CustomerClient customerClient, 
                AcumaticaOrderRepository acumaticaOrderRepository,
                AcumaticaCustomerPull acumaticaCustomerPull,
                BatchStateRepository batchStateRepository,
                
                SalesOrderClient salesOrderClient,
                TenantRepository tenantRepository,
                IPushLogger logger)
        {
            _customerClient = customerClient;
            _acumaticaOrderRepository = acumaticaOrderRepository;
            _acumaticaCustomerPull = acumaticaCustomerPull;
            _batchStateRepository = batchStateRepository;
            _salesOrderClient = salesOrderClient;
            _tenantRepository = tenantRepository;
            
            _logger = logger;
        }


        public void RunAll()
        {
            var preferences = _tenantRepository.RetrievePreferences();
            var orderUpdateMin = preferences.DataPullStart;

            var json = _salesOrderClient.RetrieveSalesOrders(orderUpdateMin);
            var orders = json.DeserializeFromJson<List<SalesOrder>>();

            UpsertOrdersToPersist(orders);

            // Set the Batch State Pull End marker
            var maxOrderDate =
                _acumaticaOrderRepository.RetrieveOrderMaxUpdatedDate();

            var batchStateEnd 
                = maxOrderDate
                    ?? DateTime.UtcNow.AddMinutes(InitialBatchStateFudgeMin);

            _batchStateRepository
                .UpdateAcumaticaOrdersPullEnd(batchStateEnd);
        }
        
        public void RunUpdated()
        {
            var batchState = _batchStateRepository.RetrieveBatchState();
            if (!batchState.AcumaticaOrdersPullEnd.HasValue)
            {
                throw new Exception(
                    "AcumaticaOrdersPullEnd is null - execute RunAll() first");
            }

            var updateMin = batchState.AcumaticaOrdersPullEnd;
            var pullRunStartTime = DateTime.UtcNow;

            var json = _salesOrderClient.RetrieveSalesOrders(updateMin);
            var orders = json.DeserializeFromJson<List<SalesOrder>>();

            UpsertOrdersToPersist(orders);

            _batchStateRepository.UpdateAcumaticaCustomersPullEnd(pullRunStartTime);
        }

        public void UpsertOrdersToPersist(List<SalesOrder> orders)
        {
            foreach (var order in orders)
            {
                UpsertOrderToPersist(order);
            }
        }

        public UsrAcumaticaSalesOrder UpsertOrderToPersist(SalesOrder order)
        {
            var orderNbr = order.OrderNbr.value;

            var existingData = _acumaticaOrderRepository.RetrieveSalesOrder(orderNbr);

            if (existingData == null)
            {
                // Locate Acumatica Customer..
                var customerId = order.CustomerID.value;

                var customerMonsterId
                    = _acumaticaCustomerPull.RunAndUpsertCustomer(customerId);

                var newData = new UsrAcumaticaSalesOrder();
                newData.AcumaticaSalesOrderId = orderNbr;
                newData.AcumaticaJson = order.SerializeToJson();
                newData.AcumaticaStatus = order.Status.value;
                newData.CustomerMonsterId = customerMonsterId;

                newData.DateCreated = DateTime.UtcNow;
                newData.LastUpdated = DateTime.UtcNow;

                _acumaticaOrderRepository.InsertSalesOrder(newData);

                return newData;
            }
            else
            {
                existingData.AcumaticaJson = order.SerializeToJson();
                existingData.AcumaticaStatus = order.Status.value;
                existingData.LastUpdated = DateTime.UtcNow;
                
                _acumaticaOrderRepository.SaveChanges();

                return existingData;
            }
        }

        
    }
}

