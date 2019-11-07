using System.Linq;
using Monster.TaxProvider.Utility;
using Newtonsoft.Json;
using PX.TaxProvider;

namespace Monster.TaxProvider.Context
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

            var contextJson = JsonConvert.SerializeObject(context);
            _logger.Info($"DocContext - {contextJson}");


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
