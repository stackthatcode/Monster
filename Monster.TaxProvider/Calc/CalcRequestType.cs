using System.Linq;
using Monster.TaxProvider.Acumatica;
using PX.TaxProvider;

namespace Monster.TaxProvider.Calc
{
    public class CalcRequestType
    {
        public CalcRequestTypeEnum Type { get; set; }
        public string RefType { get; set; }
        public string RefNbr { get; set; }
        public string DocContextTypeName => Type.ToString();

        public override string ToString()
        {
            return "";
        }

        public CalcRequestType()
        {
            Type = CalcRequestTypeEnum.Undetermined;
        }
    }

    public static class ProviderContextExtensions
    {
        public static CalcRequestType ToCalcRequestType(this GetTaxRequest request)
        {
            var parts = request.DocCode.Split('.').ToList();

            if (parts[0] == AcumaticaDocType.SalesOrder && parts[2] == AcumaticaDocType.Freight)
            {
                return new CalcRequestType
                {
                    Type = CalcRequestTypeEnum.SOFreight,
                    RefType = parts[0],
                    RefNbr = parts[1]
                };
            }

            if (parts[0] == AcumaticaDocType.SalesOrder && parts[1] == AcumaticaDocType.SalesOrder)
            {
                return new CalcRequestType
                {
                    Type = CalcRequestTypeEnum.SalesOrder,
                    RefType = parts[1],
                    RefNbr = parts[2],
                };
            }

            if (parts[0] == AcumaticaModule.AR && parts[1] == AcumaticaDocType.Invoice)
            {
                return new CalcRequestType
                {
                    Type = CalcRequestTypeEnum.SOShipmentInvoice,
                    RefType = parts[1],
                    RefNbr = parts[2],
                };
            }

            return new CalcRequestType()
            {
                Type = CalcRequestTypeEnum.Undetermined
            };
        }
    }
}


}
