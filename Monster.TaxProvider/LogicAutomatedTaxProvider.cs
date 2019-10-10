using System;
using System.Collections.Generic;
using System.Linq;
using Monster.TaxProvider.Bql;
using Monster.TaxProvider.Calc;
using Monster.TaxProvider.Helpers;
using Newtonsoft.Json;
using PX.Data;
using PX.Objects.TX;
using PX.TaxProvider;
using TaxDetail = PX.TaxProvider.TaxDetail;

namespace Monster.TaxProvider
{
    public class LogicAutomatedTaxProvider : ITaxProvider
    {
        public const string TaxProviderID = "LOGICAUTAX";
        private readonly Logger _logger = new Logger();
        public IReadOnlyList<string> Attributes => new List<string>().AsReadOnly();

        // Settings 
        //
        public const string EXTTAXREPORTER = "EXTTAXREPORTER";

        private List<ITaxProviderSetting> _settings;

        public ITaxProviderSetting[]
            DefaultSettings =>
            new List<ITaxProviderSetting>()
            {
                new TaxProviderSetting(
                    TaxProviderID,
                    EXTTAXREPORTER,
                    1,
                    "External Tax Reporting Provider",
                    string.Empty,
                    TaxProviderSettingControlType.Text),
            }.ToArray();


        // Primary worker that orchestrates Tax Transfers and Acumatica Sales Orders/Invoices
        //
        private TaxCalcService _taxCalculator;



        public void Initialize(IEnumerable<ITaxProviderSetting> settings)
        {
            _settings = settings.ToList();
            _taxCalculator = new TaxCalcService();
        }

        public PingResult Ping()
        {
            return new PingResult();
        }

        public GetTaxResult GetTax(GetTaxRequest request)
        {
            var json = JsonConvert.SerializeObject(request);
            _logger.Info($"GetTaxRequest - {json}");

            var context = DocContext.Decode(request.DocCode);
            var contextJson = JsonConvert.SerializeObject(context);
            _logger.Info($"DocContext - {contextJson}");

            if (context.TaxRequestType == TaxRequestType.SalesOrder)
            {
                var result = _taxCalculator.CalcSalesOrderLineAmountsTax(context);
                return GetTaxStub(100m);
            }
            if (context.TaxRequestType == TaxRequestType.SOFreight)
            {
                var result = _taxCalculator.CalcSalesOrderLineAmountsTax(context);
                return GetTaxStub(90m);
            }
            if (context.TaxRequestType == TaxRequestType.SOShipmentInvoice)
            {
                var result = _taxCalculator.CalcSalesOrderLineAmountsTax(context);
                return GetTaxStub(80m);
            }

            throw new ArgumentException($"Unable to process DocCode {request.DocCode}");
        }



        public GetTaxResult GetTaxStub(decimal taxAmount)
        {
            var output = new GetTaxResult();
            var taxLines = new List<TaxLine>();

            var details = new TaxDetail()
            {
                TaxName = "MANUALID",
                Rate = 0.0875m,
                TaxAmount = taxAmount,
                TaxableAmount = 1000m,
                TaxCalculationLevel = TaxCalculationLevel.CalcOnItemAmt,
            };

            taxLines.Add(new TaxLine()
            {
                Index = 1,
                Rate = 0.0875m,
                TaxAmount = taxAmount,
                TaxableAmount = 1000m,
                TaxDetails = new[] { details },
            });

            output.TaxLines = taxLines.ToArray();
            output.TotalAmount = 1000m;
            output.TotalTaxAmount = taxAmount;
            output.TaxSummary = new TaxDetail[] { details };
            output.IsSuccess = true;

            return output;
        }

        public PostTaxResult PostTax(PostTaxRequest request)
        {
            var output = new PostTaxResult();
            output.IsSuccess = true;
            return output;

            // NEED TO MAKE SURE NO RUNAWAY CALLS
            //var json = JsonConvert.SerializeObject(request);
            //_logger.Info($"PostTax - {json}");

            //var graph = new PXGraph();
            //var provider = ReportingTaxProviderFactory(graph);

            //provider.PostTax(request);
        }

        public CommitTaxResult CommitTax(CommitTaxRequest request)
        {
            var output = new CommitTaxResult();
            output.IsSuccess = true;
            return output;

            // NEED TO MAKE SURE NO RUNAWAY CALLS
            //var json = JsonConvert.SerializeObject(request);
            //_logger.Info($"CommitTax - {json}");

            //var graph = new PXGraph();
            //var provider = ReportingTaxProviderFactory(graph);

            //provider.CommitTax(request);
        }

        public VoidTaxResult VoidTax(VoidTaxRequest request)
        {
            var output = new VoidTaxResult();
            output.IsSuccess = true;
            return output;

            // NEED TO MAKE SURE NO RUNAWAY CALLS
            //var json = JsonConvert.SerializeObject(request);
            //_logger.Info($"VoidTax - {json}");

            //var graph = new PXGraph();
            //var provider = ReportingTaxProviderFactory(graph);

            //provider.VoidTax(request);
        }

        private ITaxProvider ReportingTaxProviderFactory(PXGraph graph)
        {
            var setting = _settings.FirstOrDefault(x => x.SettingID == EXTTAXREPORTER);
            if (setting == null)
            {
                throw new Exception($"Unable to locate Setting {EXTTAXREPORTER}");
            }

            var providerId = setting.Value;
            if (providerId == TaxProviderID)
            {
                throw new Exception($"{TaxProviderID} is not configured to handle Tax Reporting");
            }

            var provider = TaxPluginMaint.CreateTaxProvider(graph, providerId);
            return provider;
        }
    }
}
