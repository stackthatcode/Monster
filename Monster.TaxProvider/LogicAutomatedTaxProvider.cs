using System;
using System.Collections.Generic;
using System.Linq;
using Monster.TaxProvider.Calc;
using Monster.TaxProvider.Utility;
using Newtonsoft.Json;
using PX.Data;
using PX.Objects.TX;
using PX.TaxProvider;


namespace Monster.TaxProvider
{
    public class LogicAutomatedTaxProvider : ITaxProvider
    {
        public const string TaxProviderID = "LOGICAUTAX";
        public IReadOnlyList<string> Attributes => new List<string>().AsReadOnly();

        // Settings 
        //
        private List<ITaxProviderSetting> _settings;
        public ITaxProviderSetting[] DefaultSettings => ProviderSettings.Defaults.ToArray();


        // Dependencies
        private Logger _logger;

        private TaxCalcService _taxCalcService;


        public void Initialize(IEnumerable<ITaxProviderSetting> settings)
        {
            _settings = settings.ToList();
            _logger = new Logger(_settings.DebugLoggingEnabled());

            _taxCalcService = new TaxCalcService(_logger, _settings);
        }

        public PingResult Ping()
        {
            return new PingResult();
        }

        public GetTaxResult GetTax(GetTaxRequest request)
        {
            var json = JsonConvert.SerializeObject(request);
            _logger.Info($"GetTaxRequest ({request.DocCode}) - {json}");

            var response = _taxCalcService.Calculate(request);
            _logger.Info($"GetTaxRequest (output) - {JsonConvert.SerializeObject(response)}");

            return response;
        }

        public PostTaxResult PostTax(PostTaxRequest request)
        {
            var json = JsonConvert.SerializeObject(request);
            _logger.Info($"PostTax - {json}");

            var provider = ExternalReportingProvider();
            return provider.PostTax(request);
        }

        public CommitTaxResult CommitTax(CommitTaxRequest request)
        {
            var json = JsonConvert.SerializeObject(request);
            _logger.Info($"CommitTax - {json}");

            var provider = ExternalReportingProvider();
            return provider.CommitTax(request);
        }

        public VoidTaxResult VoidTax(VoidTaxRequest request)
        {
            var json = JsonConvert.SerializeObject(request);
            _logger.Info($"VoidTax - {json}");

            var provider = ExternalReportingProvider();
            return provider.VoidTax(request);
        }

        private ITaxProvider ExternalReportingProvider()
        {
            var graph = new PXGraph();
            var setting = _settings.Setting(ProviderSettings.SETTING_EXTTAXREPORTER);
            
            var providerId = setting.Value;
            _logger.Info($"ExternalReportingProvider - {providerId}");

            if (providerId == TaxProviderID)
            {
                throw new Exception($"{TaxProviderID} is not configured to handle Tax Reporting");
            }

            var provider = TaxPluginMaint.CreateTaxProvider(graph, providerId);
            if (provider is LogicAutomatedTaxProvider)
            {
                throw new Exception($"{TaxProviderID} is not configured to handle Tax Reporting");
            }

            return provider;
        }
    }
}
