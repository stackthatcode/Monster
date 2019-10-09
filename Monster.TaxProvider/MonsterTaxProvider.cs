using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using PX.Data;
using PX.Objects.SO;
using PX.Objects.TX;
using PX.TaxProvider;
using TaxDetail = PX.TaxProvider.TaxDetail;

namespace Monster.TaxProvider
{
    public class MonsterTaxProvider : ITaxProvider
    {
        public const string TaxProviderID = "JONESTAX";
        private List<ITaxProviderSetting> _settings;

        public void Initialize(IEnumerable<ITaxProviderSetting> settings)
        {
            _settings = settings.ToList();
        }

        public PingResult Ping()
        {
            return new PingResult();
        }

        public ITaxProvider ReportingTaxProvider(PXGraph graph)
        {
            var provider = TaxPluginMaint.CreateTaxProvider(graph, "JONESTAX");
            return provider;
        }

        public GetTaxResult GetTax(GetTaxRequest request)
        {
            var json = JsonConvert.SerializeObject(request);
            var output = new GetTaxResult();

            PXTrace.WriteInformation($"Tax Connector - invoking for {request.DocCode}");
            PXTrace.WriteInformation("Tax Connector - " + json);

            // If this is for an Invoice then 
            var graph = new PXGraph();

            var provider = ReportingTaxProvider(graph);

            var shipment =
               ((SOOrderShipment)
                   PXSelect<SOOrderShipment,
                       Where<SOOrderShipment.invoiceNbr,
                           Equal<Required<SOOrderShipment.invoiceNbr>>>>
                   .Select(graph, "000016"));
            //.InvoiceType;

            var salesOrder =
                ((SOOrder)PXSelect<SOOrder,
                        Where<SOOrder.orderNbr, Equal<Required<SOOrder.orderNbr>>>>
                        .Select(graph, "000004"));

            var salesOrderExt = PXCache<SOOrder>.GetExtension<SOOrderTaxSnapshotExt>(salesOrder);
            var snapshot = salesOrderExt.UsrTaxSnapshot;

            //var salesOrderExt = salesOrder.GetExtension<SOOrderTaxSnapshotExt>();
            PXTrace.WriteInformation($"Custom field value {snapshot}");

            //PXTrace.WriteInformation($"Tax Connector - Invoice Type Test {invoiceType}");

            var taxLines = new List<TaxLine>();

            var details = new TaxDetail()
            {
                TaxName = "MANUALID",
                Rate = 0.0875m,
                TaxAmount = 87.50m,
                TaxableAmount = 1000m,
                TaxCalculationLevel = TaxCalculationLevel.CalcOnItemAmt,
            };

            taxLines.Add(new TaxLine()
            {
                Index = 1,
                Rate = 0.0875m,
                TaxAmount = 87.50m,
                TaxableAmount = 1000m,
                TaxDetails = new[] { details },
            });

            output.TaxLines = taxLines.ToArray();
            output.TotalAmount = 1000m;
            output.TotalTaxAmount = 87.50m;
            output.TaxSummary = new TaxDetail[] { details };
            output.IsSuccess = true;

            return output;
        }


        public PostTaxResult PostTax(PostTaxRequest request)
        {
            var output = new PostTaxResult();
            output.IsSuccess = true;
            return output;
        }

        public CommitTaxResult CommitTax(CommitTaxRequest request)
        {
            var output = new CommitTaxResult();
            output.IsSuccess = true;
            return output;
        }

        public VoidTaxResult VoidTax(VoidTaxRequest request)
        {
            var output = new VoidTaxResult();
            output.IsSuccess = true;
            return output;
        }

        public IReadOnlyList<string> Attributes => new List<string>().AsReadOnly();

        public ITaxProviderSetting[]
            DefaultSettings =>
                new List<ITaxProviderSetting>()
                {
                    new TaxProviderSetting(
                        "JONESTAX",
                        "TAXREPORTINGPROVIDER",
                        1,
                        "Tax Reporting Provider",
                        string.Empty,
                        TaxProviderSettingControlType.Text),
                }.ToArray();
    }
}
