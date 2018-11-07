using System;
using System.Linq;

namespace Monster.Middle.Persist.Multitenant.Acumatica
{
    public class AcumaticaOrderRepository
    {
        private readonly PersistContext _dataContext;
        public MonsterDataContext Entities => _dataContext.Entities;

        public AcumaticaOrderRepository(PersistContext dataContext)
        {
            _dataContext = dataContext;
        }


        // Acumatica
        //
        public UsrAcumaticaCustomer RetrieveCustomer(string acumaticaCustomerId)
        {
            return Entities
                .UsrAcumaticaCustomers
                .FirstOrDefault(
                    x => x.AcumaticaCustomerId == acumaticaCustomerId);
        }

        public UsrAcumaticaCustomer RetrieveCustomerByEmail(string email)
        {
            return Entities
                .UsrAcumaticaCustomers
                .FirstOrDefault(
                    x => x.AcumaticaMainContactEmail == email);
        }


        public DateTime? RetrieveCustomerMaxUpdatedDate()
        {
            if (Entities.UsrAcumaticaCustomers.Any())
            {
                return Entities.UsrAcumaticaCustomers
                    .Select(x => x.LastUpdated)
                    .Max();
            }
            else
            {
                return (DateTime?)null;
            }
        }

        public void InsertCustomer(UsrAcumaticaCustomer customer)
        {
            Entities.UsrAcumaticaCustomers.Add(customer);
            Entities.SaveChanges();
        }
        

        // Sales Order
        public UsrAcumaticaSalesOrder RetrieveSalesOrder(string acumaticaSOId)
        {
            return Entities
                .UsrAcumaticaSalesOrders
                .FirstOrDefault(x => x.AcumaticaSalesOrderId == acumaticaSOId);
        }
        
        public DateTime? RetrieveOrderMaxUpdatedDate()
        {
            if (Entities.UsrAcumaticaCustomers.Any())
            {
                return Entities.UsrAcumaticaSalesOrders
                    .Select(x => x.LastUpdated)
                    .Max();
            }
            else
            {
                return (DateTime?)null;
            }
        }

        public void InsertSalesOrder(UsrAcumaticaSalesOrder order)
        {
            Entities.UsrAcumaticaSalesOrders.Add(order);
            Entities.SaveChanges();
        }


        // Shipment
        public UsrAcumaticaShipment RetrieveShipment(string shipmentNbr)
        {
            return Entities
                .UsrAcumaticaShipments
                .FirstOrDefault(x => x.AcumaticaShipmentId == shipmentNbr);
        }

        public void InsertShipment(UsrAcumaticaShipment shipment)
        {
            Entities.UsrAcumaticaShipments.Add(shipment);
            Entities.SaveChanges();
        }

        public void SaveChanges()
        {
            Entities.SaveChanges();
        }
    }
}
