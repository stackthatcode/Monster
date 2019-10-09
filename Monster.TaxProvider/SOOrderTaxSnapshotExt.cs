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
    public class SOOrderTaxSnapshotExt : PXCacheExtension<SOOrder>
    {
        [PXDBString(4000)]
        [PXUIField(DisplayName = "External Tax Snapshot")]
        public virtual string UsrTaxSnapshot { get; set; }
        public abstract class usrTaxSnapshot : IBqlField { }
    }

}

