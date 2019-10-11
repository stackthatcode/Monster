using System.Linq;
using PX.TaxProvider;

namespace Monster.TaxProvider.Context
{
    public class DocContext
    {
        public DocContextType DocContextType { get; private set; }
        public string DocContextTypeName => DocContextType.ToString();
        public string RefType { get; private set; }
        public string RefNbr { get; private set; }

        public DocContext()
        {
            DocContextType = DocContextType.Undetermined;
        }


        public static DocContext ExtractContext(GetTaxRequest request)
        {
            var parts = request.DocCode.Split('.').ToList();

            if (parts[0] == AcumaticaDocType.SalesOrder && parts[2] == AcumaticaDocType.Freight)
            {
                return new DocContext
                {
                    DocContextType = DocContextType.SOFreight,
                    RefType = parts[0],
                    RefNbr = parts[1]
                };
            }

            if (parts[0] == AcumaticaDocType.SalesOrder && parts[1] == AcumaticaDocType.SalesOrder)
            {
                return new DocContext
                {
                    DocContextType = DocContextType.SalesOrder,
                    RefType = parts[1],
                    RefNbr = parts[2],
                };
            }

            if (parts[0] == AcumaticaModule.AR && parts[1] == AcumaticaDocType.Invoice)
            {
                return new DocContext
                {
                    DocContextType = DocContextType.SOShipmentInvoice,
                    RefType = parts[1],
                    RefNbr = parts[2],
                };
            }

            return new DocContext()
            {
                DocContextType = DocContextType.Undetermined
            };
        }
    }
}
