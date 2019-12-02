using System.Linq;
using Monster.TaxProvider.Acumatica;
using PX.TaxProvider;

namespace Monster.TaxProvider.Calc
{
    public class CalcRequestContext
    {
        public CalcRequestTypeEnum Type { get; set; }
        public string DocContextTypeName => Type.ToString();

        public string OrderType { get; set; }
        public string OrderNbr { get; set; }
        public string InvoiceType { get; set; }
        public string InvoiceNbr { get; set; }

        public override string ToString()
        {
            return "";
        }

        public CalcRequestContext()
        {
            Type = CalcRequestTypeEnum.Undetermined;
        }
    }

    public static class ProviderContextExtensions
    {
        public static CalcRequestContext ToCalcRequestContext(this GetTaxRequest request)
        {
            var parts = request.DocCode.Split('.').ToList();

            if (parts[0] == AcumaticaDocType.SalesOrder && parts[2] == AcumaticaDocType.Freight)
            {
                return new CalcRequestContext
                {
                    Type = CalcRequestTypeEnum.SOFreight,
                    OrderType = parts[0],
                    OrderNbr = parts[1]
                };
            }

            if (parts[0] == AcumaticaDocType.SalesOrder && parts[2] == AcumaticaDocType.Open)
            {
                return new CalcRequestContext
                {
                    Type = CalcRequestTypeEnum.SalesOrder,
                    OrderType = parts[0],
                    OrderNbr = parts[1]
                };
            }

            if (parts[0] == AcumaticaDocType.SalesOrder && parts[1] == AcumaticaDocType.SalesOrder)
            {
                return new CalcRequestContext
                {
                    Type = CalcRequestTypeEnum.SalesOrder,
                    OrderType = parts[1],
                    OrderNbr = parts[2]
                };
            }

            if (parts[0] == AcumaticaModule.AR && parts[1] == AcumaticaDocType.Invoice)
            {
                return new CalcRequestContext
                {
                    Type = CalcRequestTypeEnum.SOShipmentInvoice,
                    InvoiceType = parts[1],
                    InvoiceNbr = parts[2],
                };
            }

            return new CalcRequestContext()
            {
                Type = CalcRequestTypeEnum.Undetermined
            };
        }
    }
}

