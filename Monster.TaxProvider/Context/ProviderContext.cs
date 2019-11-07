using System.Linq;
using PX.TaxProvider;

namespace Monster.TaxProvider.Context
{
    public class ProviderContext
    {
        public ProviderContextType DocContextType { get;  set; }
        public string RefType { get; set; }
        public string RefNbr { get; set; }

        public string DocContextTypeName => DocContextType.ToString();

        public ProviderContext()
        {
            DocContextType = ProviderContextType.Undetermined;
        }
    }
}
