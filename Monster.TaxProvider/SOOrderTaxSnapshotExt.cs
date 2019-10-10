using PX.Data;
using PX.Objects.SO;


namespace Monster.TaxProvider
{
    public class SOOrderTaxSnapshotExt : PXCacheExtension<SOOrder>
    {
        [PXDBString()]
        [PXUIField(DisplayName = "External Tax Snapshot")]
        public virtual string UsrTaxSnapshot { get; set; }
        public abstract class usrTaxSnapshot : IBqlField { }
    }

}

