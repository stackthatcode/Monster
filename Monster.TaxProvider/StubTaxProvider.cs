using System.Collections.Generic;
using System.Linq;
using Monster.TaxProvider.Utility;
using PX.TaxProvider;

namespace Monster.TaxProvider
{
    public class StubTaxProvider : ITaxProvider
    {
        public const string TaxProviderID = "STUBTAX";
        private readonly Logger _logger = new Logger(false);
        public IReadOnlyList<string> Attributes => new List<string>().AsReadOnly();

        private List<ITaxProviderSetting> _settings;

        public ITaxProviderSetting[] DefaultSettings => new List<ITaxProviderSetting>().ToArray();


        public void Initialize(IEnumerable<ITaxProviderSetting> settings)
        {
            _settings = settings.ToList();
        }

        public PingResult Ping()
        {
            _logger.Info("StubTaxProvider - Ping");
            return new PingResult();
        }

        public GetTaxResult GetTax(GetTaxRequest request)
        {
            _logger.Info("StubTaxProvider - GetTax");
            return GetTaxResult.Empty;
        }

        public PostTaxResult PostTax(PostTaxRequest request)
        {
            _logger.Info("StubTaxProvider - PostTax");
            return new PostTaxResult() { IsSuccess = true };
        }

        public CommitTaxResult CommitTax(CommitTaxRequest request)
        {
            _logger.Info("StubTaxProvider - CommitTax");
            return new CommitTaxResult() { IsSuccess = true };
        }

        public VoidTaxResult VoidTax(VoidTaxRequest request)
        {
            _logger.Info("StubTaxProvider - VoidTax");
            return new VoidTaxResult() { IsSuccess = true };
        }
    }
}
