using System.Linq;
using Monster.Acumatica.Api.SalesOrder;

namespace Monster.Middle.Processes.Sync.Model.Extensions
{
    public static class ShipmentExtensions
    {

        public static bool HasInvoicedShipment(this SalesOrder salesOrder)
        {
            return salesOrder.Shipments.Any(x => x.InvoiceNbr != null);
        }

        public static string ShipmentInvoiceNbr(this SalesOrder salesOrder)
        {
            return salesOrder.Shipments.First().InvoiceNbr.value;
        }

        public static bool IsInvoiceReleased(this SalesOrder salesOrder)
        {
            return salesOrder.Status.value == Acumatica.Persist.Status.Completed;
        }

    }
}
