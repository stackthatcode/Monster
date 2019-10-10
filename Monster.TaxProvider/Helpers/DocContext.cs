using System;
using System.Linq;

namespace Monster.TaxProvider.Helpers
{
    public class DocContext
    {
        public bool IsFreight { get; private set; }
        public bool IsInvoice { get; private set; }
        public bool IsSalesOrder { get; private set; }

        public string RefType { get; private set; }
        public string RefNbr { get; private set; }


        public static DocContext Decode(string docCode)
        {
            var parts = docCode.Split('.').ToList();

            if (parts[0] == DocType.SalesOrder && parts[2] == "Freight")
            {
                return new DocContext
                {
                    IsFreight = true,
                    RefType = parts[0],
                    RefNbr = parts[1]
                };
            }

            if (parts[0] == DocType.SalesOrder && parts[0] == DocType.SalesOrder)
            {
                return new DocContext
                {
                    IsFreight = false,
                    RefType = parts[1],
                    RefNbr = parts[2],
                };
            }

            if (parts[0] == Module.AR && parts[1] == DocType.SalesOrder)
            {
                return new DocContext
                {
                    IsFreight = false,
                    RefType = parts[1],
                    RefNbr = parts[2],
                };
            }

            throw new ArgumentException($"DocCode {docCode} is currently not supported.");
        }
    }
}
