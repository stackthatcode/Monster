using Monster.Acumatica.Api.SalesOrder;

namespace Monster.Middle.Processes.Sync.Model.Extensions
{
    public static class InvoiceExtensions
    {
        public static bool IsReleased(this SalesInvoice input)
        {
            return input.Status.value == Acumatica.Persist.Status.Open;
        }
    }
}

