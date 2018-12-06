﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Monster.Middle.Persist.Multitenant;

namespace Monster.Middle.Processes.Acumatica.Persist
{
    public class AcumaticaOrderRepository
    {
        private readonly PersistContext _dataContext;
        public MonsterDataContext Entities => _dataContext.Entities;

        public AcumaticaOrderRepository(PersistContext dataContext)
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
        //
        public UsrAcumaticaSalesOrder RetrieveSalesOrder(string acumaticaOrderNbr)
        {
            return Entities
                .UsrAcumaticaSalesOrders
                .Include(x => x.UsrShopAcuOrderSyncs)
                .FirstOrDefault(x => x.AcumaticaOrderNbr == acumaticaOrderNbr);
        }

        public DateTime? RetrieveOrderMaxUpdatedDate()
        {
            if (Entities.UsrAcumaticaSalesOrders.Any())
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


        // Shipments
        //
        public UsrAcumaticaShipment RetrieveShipment(string shipmentNbr)
        {
            return Entities
                .UsrAcumaticaShipments
                .Include(x => x.UsrAcumaticaShipmentDetails)
                .Include(x => x.UsrAcumaticaShipmentDetails.Select(y => y.UsrShopAcuShipmentSyncs))
                .FirstOrDefault(x => x.AcumaticaShipmentNbr == shipmentNbr);
        }

        public UsrAcumaticaShipment RetrieveShipment(long shipmentMonsterId)
        {
            return Entities
                .UsrAcumaticaShipments
                .Include(x => x.UsrAcumaticaShipmentDetails)
                .Include(x => x.UsrAcumaticaShipmentDetails.Select(y => y.UsrShopAcuShipmentSyncs))
                .FirstOrDefault(x => x.Id == shipmentMonsterId);
        }

        public void InsertShipment(UsrAcumaticaShipment shipment)
        {
            Entities.UsrAcumaticaShipments.Add(shipment);
            Entities.SaveChanges();
        }

        public DateTime? RetrieveShipmentMaxUpdatedDate()
        {
            if (Entities.UsrAcumaticaShipments.Any())
            {
                return Entities.UsrAcumaticaShipments
                    .Select(x => x.LastUpdated)
                    .Max();
            }
            else
            {
                return (DateTime?)null;
            }
        }

        public List<long> RetrieveUnsyncedShipmentIds()
        {
            var sql = @"SELECT * FROM vw_AcumaticaUnsyncedShipmentIds";
            return Entities
                .Database
                .SqlQuery<long>(sql)
                .ToList();
        }
        
        public void ImprintShipmentDetail(
                long monsterShipmentId, List<UsrAcumaticaShipmentDetail> newestRecords)
        {
            var existingRecords =
                Entities
                    .UsrAcumaticaShipmentDetails
                    .Where(x => x.ShipmentMonsterId == monsterShipmentId)
                    .ToList();
            
            foreach (var existingRecord in existingRecords)
            {
                if (!newestRecords.AnyMatch(existingRecord))
                {
                    Entities.UsrAcumaticaShipmentDetails.Remove(existingRecord);
                    Entities.SaveChanges();
                }
            }

            foreach (var newestRecord in newestRecords)
            {
                if (!existingRecords.AnyMatch(newestRecord))
                {
                    newestRecord.ShipmentMonsterId = monsterShipmentId;
                    newestRecord.DateCreated = DateTime.UtcNow;
                    newestRecord.LastUpdated = DateTime.UtcNow;

                    Entities.UsrAcumaticaShipmentDetails.Add(newestRecord);
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
