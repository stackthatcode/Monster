using System;

namespace Monster.MiddleTier.Persist.System
{
    public class Tenant
    {
        public Guid TenantId { get; set; }
        public string ConnectionString { get; set; }
        public long CompanyId { get; set; }
    }
}
