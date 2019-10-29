using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Monster.Middle.Persist.Instance;
using Monster.Middle.Persist.Instance;

namespace Monster.Middle.Processes.Acumatica.Persist
{
    public class AcumaticaOrderRepository
    {
        private readonly ProcessPersistContext _dataContext;
        public MonsterDataContext Entities => _dataContext.Entities;

        public AcumaticaOrderRepository(ProcessPersistContext dataContext)
        {
            _dataContext = dataContext;
        }


        public DbContextTransaction BeginTransaction()
        {
            return Entities.Database.BeginTransaction();
        }

        // Acumatica Customer
        //
        public AcumaticaCustomer RetrieveCustomer(string acumaticaCustomerId)
        {
            return Entities
                .AcumaticaCustomers
                .FirstOrDefault(
                    x => x.AcumaticaCustomerId == acumaticaCustomerId);
        }

        public AcumaticaCustomer RetrieveCustomerByEmail(string email)
        {
            return Entities
                .AcumaticaCustomers
                .FirstOrDefault(
                    x => x.AcumaticaMainContactEmail == email);
        }
        
        public void InsertCustomer(AcumaticaCustomer customer)
        {
            Entities.AcumaticaCustomers.Add(customer);
            Entities.SaveChanges();
        }

        public void UpdateCustomer(string customerId)
        {
            var customer = Entities.AcumaticaCustomers.First(x => x.AcumaticaCustomerId == customerId);
            Entities.SaveChanges();
        }



        // Sales Order
        //
        public AcumaticaSalesOrder RetrieveSalesOrder(string acumaticaOrderNbr)
        {
            return Entities
                .AcumaticaSalesOrders
                .Include(x => x.ShopAcuOrderSyncs)
                .FirstOrDefault(x => x.AcumaticaOrderNbr == acumaticaOrderNbr);
        }


        public void InsertSalesOrder(AcumaticaSalesOrder order)
        {
            Entities.AcumaticaSalesOrders.Add(order);
            Entities.SaveChanges();
        }

        public bool ExistsSoShipmentInvoice(long monsterSalesOrderId, string shipmentNbr, string invoiceNbr)
        {
            return Entities
                .AcumaticaSoShipments
                .Any(x => x.AcumaticaSalesOrder.Id == monsterSalesOrderId 
                         && x.AcumaticaInvoiceNbr == invoiceNbr 
                         && x.AcumaticaShipmentNbr == shipmentNbr);
        }

        public void InsertSoShipmentInvoice(AcumaticaSoShipment record)
        {
            Entities.AcumaticaSoShipments.Add(record);
            Entities.SaveChanges();
        }



        public void SaveChanges()
        {
            Entities.SaveChanges();
        }

    }
}
