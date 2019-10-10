using System;
using System.Linq;

namespace Monster.TaxProvider.Helpers
{
    public class DocContext
    {
        public TaxRequestType TaxRequestType { get; set; }
        public string TaxRequestTypeName => TaxRequestType.ToString();
        public string RefType { get; private set; }
        public string RefNbr { get; private set; }

        public DocContext()
        {
            TaxRequestType = TaxRequestType.Undetermined;
        }


        public static DocContext Decode(string docCode)
        {
            var parts = docCode.Split('.').ToList();

            if (parts[0] == DocType.SalesOrder && parts[2] == "Freight")
            {
                return new DocContext
                {
                    TaxRequestType = TaxRequestType.SalesOrder,
                    RefType = parts[0],
                    RefNbr = parts[1]
                };
            }

            if (parts[0] == DocType.SalesOrder && parts[0] == DocType.SalesOrder)
            {
                return new DocContext
                {
                    TaxRequestType = TaxRequestType.SOFreight,
                    RefType = parts[1],
                    RefNbr = parts[2],
                };
            }

            if (parts[0] == Module.AR && parts[1] == DocType.SalesOrder)
            {
                return new DocContext
                {
                    TaxRequestType = TaxRequestType.SOShipmentInvoice,
                    RefType = parts[1],
                    RefNbr = parts[2],
                };
            }

            throw new ArgumentException($"DocCode {docCode} is currently not supported.");
        }
    }
}

