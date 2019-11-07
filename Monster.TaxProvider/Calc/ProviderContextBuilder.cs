using System.Linq;
using Monster.TaxProvider.Acumatica;
using Monster.TaxProvider.Context;
using Monster.TaxProvider.Utility;
using PX.TaxProvider;

namespace Monster.TaxProvider.Calc
{
    public class ProviderContextBuilder
    {
        private readonly Logger _logger;

        public ProviderContextBuilder(Logger logger)
        {
            _logger = logger;
        }

        public ProviderContext ExtractFromRequest(GetTaxRequest request)
        {
            var parts = request.DocCode.Split('.').ToList();

            if (parts[0] == AcumaticaDocType.SalesOrder && parts[2] == AcumaticaDocType.Freight)
            {
                return new ProviderContext
                {
                    DocContextType = ProviderContextType.SOFreight,
                    RefType = parts[0],
                    RefNbr = parts[1]
                };
            }

            if (parts[0] == AcumaticaDocType.SalesOrder && parts[1] == AcumaticaDocType.SalesOrder)
            {
                return new ProviderContext
                {
                    DocContextType = ProviderContextType.SalesOrder,
                    RefType = parts[1],
                    RefNbr = parts[2],
                };
            }

            if (parts[0] == AcumaticaModule.AR && parts[1] == AcumaticaDocType.Invoice)
            {
                return new ProviderContext
                {
                    DocContextType = ProviderContextType.SOShipmentInvoice,
                    RefType = parts[1],
                    RefNbr = parts[2],
                };
            }

            return new ProviderContext()
            {
                DocContextType = ProviderContextType.Undetermined
            };
        }
    }
}
