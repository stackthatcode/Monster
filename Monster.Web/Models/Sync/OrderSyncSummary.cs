namespace Monster.Web.Models.Sync
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

