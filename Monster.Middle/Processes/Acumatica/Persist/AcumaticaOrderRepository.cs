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

        public void ImprintSoShipmentInvoices(
                long monsterSalesOrderId, 
                List<AcumaticaSoShipmentInvoice> newestRecords)
        {
            var existingRecords =
                Entities
                    .AcumaticaSoShipmentInvoices
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
                    Entities.AcumaticaSoShipmentInvoices.Remove(existingRecord);
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

                    Entities.AcumaticaSoShipmentInvoices.Add(newestRecord);
                    Entities.SaveChanges();
                }
            }
        }
        



        // Shipments
        //
        public bool ShipmentExists(string shipmentNbr)
        {
            return Entities
                .AcumaticaShipments
                .Any(x => x.AcumaticaShipmentNbr == shipmentNbr);
        }
        
        public AcumaticaShipment RetrieveShipment(string shipmentNbr)
        {
            return Entities
                .AcumaticaShipments
                .Include(x => x.AcumaticaShipmentSalesOrderRefs)
                .Include(x => x.AcumaticaShipmentSalesOrderRefs.Select(y => y.ShopAcuShipmentSyncs))
                .FirstOrDefault(x => x.AcumaticaShipmentNbr == shipmentNbr);
        }

        public AcumaticaShipment RetrieveShipment(long shipmentMonsterId)
        {
            return Entities
                .AcumaticaShipments
                .Include(x => x.AcumaticaShipmentSalesOrderRefs)
                .Include(x => x.AcumaticaShipmentSalesOrderRefs.Select(y => y.ShopAcuShipmentSyncs))
                .FirstOrDefault(x => x.Id == shipmentMonsterId);
        }
        
        public void InsertShipment(AcumaticaShipment shipment)
        {
            Entities.AcumaticaShipments.Add(shipment);
            Entities.SaveChanges();
        }


        // These don't necessarily belong here
        //
        public List<AcumaticaShipmentSalesOrderRef> 
                                RetrieveUnsyncedShipmentSalesOrderRefs()
        {            
            return Entities
                .AcumaticaShipmentSalesOrderRefs
                .Include(x => x.AcumaticaShipment)
                .Where(x => 
                    x.AcumaticaShipment.AcumaticaStatus == Status.Completed)
                .Where(x => !Entities
                                .ShopAcuShipmentSyncs
                                .Select(y => y.AcumaticaShipDetailMonsterId)
                                .Contains(x.Id))
                .ToList();
        }

        public bool IsMonsterSyncedOrder(string orderNbr)
        {
            return Entities
                .AcumaticaSalesOrders
                .Any(x => x.AcumaticaOrderNbr == orderNbr
                          && x.ShopAcuOrderSyncs.Any());
        }


        public void ImprintShipmentOrderRefs(
                long monsterShipmentId, 
                List<AcumaticaShipmentSalesOrderRef> newestRecords)
        {
            var existingRecords =
                Entities
                    .AcumaticaShipmentSalesOrderRefs
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
                    Entities.AcumaticaShipmentSalesOrderRefs.Remove(existingRecord);
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

                    Entities.AcumaticaShipmentSalesOrderRefs.Add(newestRecord);
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
