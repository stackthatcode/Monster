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
        private readonly InstancePersistContext _dataContext;
        public MonsterDataContext Entities => _dataContext.Entities;

        public AcumaticaOrderRepository(InstancePersistContext dataContext)
        {
            _dataContext = dataContext;
        }


        public DbContextTransaction BeginTransaction()
        {
            return Entities.Database.BeginTransaction();
        }

        // Acumatica Customer
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
        
        public void InsertCustomer(UsrAcumaticaCustomer customer)
        {
            Entities.UsrAcumaticaCustomers.Add(customer);
            Entities.SaveChanges();
        }
        
        

        // Sales Order
        //
        public UsrAcumaticaSalesOrder RetrieveSalesOrder(string acumaticaOrderNbr)
        {
            return Entities
                .UsrAcumaticaSalesOrders
                .Include(x => x.UsrShopAcuOrderSyncs)
                .FirstOrDefault(x => x.AcumaticaOrderNbr == acumaticaOrderNbr);
        }


        public void InsertSalesOrder(UsrAcumaticaSalesOrder order)
        {
            Entities.UsrAcumaticaSalesOrders.Add(order);
            Entities.SaveChanges();
        }

        public void ImprintSoShipmentInvoices(
                long monsterSalesOrderId, 
                List<UsrAcumaticaSoShipmentInvoice> newestRecords)
        {
            var existingRecords =
                Entities
                    .UsrAcumaticaSoShipmentInvoices
                    .Where(x => x.SalesOrderMonsterId == monsterSalesOrderId)
                    .ToList();

            foreach (var existingRecord in existingRecords)
            {
                var newestMatch = newestRecords.Match(existingRecord);

                if (newestMatch != null)
                {
                    existingRecord.AcumaticaInvoiceNbr = newestMatch.AcumaticaInvoiceNbr;
                    existingRecord.IsLatestPulled = false;
                    existingRecord.LastUpdated = DateTime.Now;
                }
                else
                {
                    Entities.UsrAcumaticaSoShipmentInvoices.Remove(existingRecord);
                    Entities.SaveChanges();
                }
            }

            foreach (var newestRecord in newestRecords)
            {
                if (!existingRecords.AnyMatch(newestRecord))
                {
                    newestRecord.SalesOrderMonsterId = monsterSalesOrderId;
                    newestRecord.DateCreated = DateTime.UtcNow;
                    newestRecord.LastUpdated = DateTime.UtcNow;
                    newestRecord.IsLatestPulled = false;

                    Entities.UsrAcumaticaSoShipmentInvoices.Add(newestRecord);
                    Entities.SaveChanges();
                }
            }
        }
        



        // Shipments
        //
        public bool ShipmentExists(string shipmentNbr)
        {
            return Entities
                .UsrAcumaticaShipments
                .Any(x => x.AcumaticaShipmentNbr == shipmentNbr);
        }
        
        public UsrAcumaticaShipment RetrieveShipment(string shipmentNbr)
        {
            return Entities
                .UsrAcumaticaShipments
                .Include(x => x.UsrAcumaticaShipmentSalesOrderRefs)
                .Include(x => x.UsrAcumaticaShipmentSalesOrderRefs.Select(y => y.UsrShopAcuShipmentSyncs))
                .FirstOrDefault(x => x.AcumaticaShipmentNbr == shipmentNbr);
        }

        public UsrAcumaticaShipment RetrieveShipment(long shipmentMonsterId)
        {
            return Entities
                .UsrAcumaticaShipments
                .Include(x => x.UsrAcumaticaShipmentSalesOrderRefs)
                .Include(x => x.UsrAcumaticaShipmentSalesOrderRefs.Select(y => y.UsrShopAcuShipmentSyncs))
                .FirstOrDefault(x => x.Id == shipmentMonsterId);
        }
        
        public void InsertShipment(UsrAcumaticaShipment shipment)
        {
            Entities.UsrAcumaticaShipments.Add(shipment);
            Entities.SaveChanges();
        }


        // These don't necessarily belong here
        //
        public List<UsrAcumaticaShipmentSalesOrderRef> 
                                RetrieveUnsyncedShipmentSalesOrderRefs()
        {            
            return Entities
                .UsrAcumaticaShipmentSalesOrderRefs
                .Include(x => x.UsrAcumaticaShipment)
                .Where(x => 
                    x.UsrAcumaticaShipment.AcumaticaStatus == Status.Completed)
                .Where(x => !Entities
                                .UsrShopAcuShipmentSyncs
                                .Select(y => y.AcumaticaShipDetailMonsterId)
                                .Contains(x.Id))
                .ToList();
        }

        public bool IsMonsterSyncedOrder(string orderNbr)
        {
            return Entities
                .UsrAcumaticaSalesOrders
                .Any(x => x.AcumaticaOrderNbr == orderNbr
                          && x.UsrShopAcuOrderSyncs.Any());
        }


        public void ImprintShipmentOrderRefs(
                long monsterShipmentId, 
                List<UsrAcumaticaShipmentSalesOrderRef> newestRecords)
        {
            var existingRecords =
                Entities
                    .UsrAcumaticaShipmentSalesOrderRefs
                    .Where(x => x.ShipmentMonsterId == monsterShipmentId)
                    .ToList();
            
            foreach (var existingRecord in existingRecords)
            {
                if (newestRecords.AnyMatch(existingRecord))
                {
                    existingRecord.LastUpdated = DateTime.UtcNow;
                }
                else
                {
                    Entities.UsrAcumaticaShipmentSalesOrderRefs.Remove(existingRecord);
                    Entities.SaveChanges();
                }
            }

            foreach (var newestRecord in newestRecords)
            {
                if (!existingRecords.AnyMatch(newestRecord))
                {
                    newestRecord.ShipmentMonsterId = monsterShipmentId;
                    //newestRecord.IsMonsterOrder = true;
                    newestRecord.DateCreated = DateTime.UtcNow;
                    newestRecord.LastUpdated = DateTime.UtcNow;

                    Entities.UsrAcumaticaShipmentSalesOrderRefs.Add(newestRecord);
                    Entities.SaveChanges();
                }                
            }
        }



        public void SaveChanges()
        {
            Entities.SaveChanges();
        }

    }
}
