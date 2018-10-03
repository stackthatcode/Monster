using System;

namespace Monster.Middle.Persist.Sys
{
    public class Tenant
    {
        public Guid TenantId { get; set; }
        public string ConnectionString { get; set; }
        public long CompanyId { get; set; }
        public string Nickname { get; set; }
    }
}