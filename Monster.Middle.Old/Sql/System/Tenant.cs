using System;

namespace Monster.Middle.Persistence.System
{
    public class Tenant
    {
        public Guid TenantId { get; set; }
        public string ConnectionString { get; set; }
        public long CompanyId { get; set; }
    }
}
