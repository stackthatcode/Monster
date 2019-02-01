using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Monster.Web.Models.RealTime
{
    public class OrderSyncSummary
    {
        public int TotalOrders { get; set; }
        public int TotalOrdersWithSalesOrders { get; set; }
        public int TotalOrdersWithShipments { get; set; }
        public int TotalOrdersWithInvoices { get; set; }

        public bool SalesOrdersSynced => TotalOrders == TotalOrdersWithSalesOrders;
        public bool SalesOrdersShipped => TotalOrders == TotalOrdersWithShipments;
        public bool SalesOrdersInvoiced => TotalOrders == TotalOrdersWithInvoices;
        
    }
}

