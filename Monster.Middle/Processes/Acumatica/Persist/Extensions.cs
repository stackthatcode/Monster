using System;
using Monster.Acumatica.Api.SalesOrder;
using Monster.Middle.Persist.Instance;
using Push.Foundation.Utilities.Json;

namespace Monster.Middle.Processes.Acumatica.Persist
{
    public static class Extensions
    {
        public const int FudgeFactorMinutes = -10;

        public static DateTime AddAcumaticaBatchFudge(this DateTime input)
        {
            return input.AddMinutes(FudgeFactorMinutes);
        }

        public static SalesOrder ToSalesOrderObj(this string json)
        {
            return json.DeserializeFromJson<SalesOrder>();
        }

        public static SalesInvoice ToSalesOrderInvoiceObj(this string json)
        {
            return json.DeserializeFromJson<SalesInvoice>();
        }

        public static void Ingest(this AcumaticaSalesOrder target, SalesOrder source)
        {
            target.AcumaticaOrderNbr = source.OrderNbr.value;
            target.AcumaticaStatus = source.Status.value;
            target.AcumaticaIsTaxValid = source.IsTaxValid.value;
            target.AcumaticaLineTotal = (decimal)source.Totals.LineTotalAmount.value;
            target.AcumaticaFreight = (decimal)source.Totals.Freight.value;
            target.AcumaticaTaxTotal = (decimal)source.Totals.TaxTotal.value;
            target.AcumaticaOrderTotal = (decimal)source.OrderTotal.value;
            target.AcumaticaQtyTotal = (int)source.OrderedQty.value;
        }
    }
}

